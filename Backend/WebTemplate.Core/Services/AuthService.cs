using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Web;
using WebTemplate.Core.DTOs;
using WebTemplate.Core.DTOs.Auth;
using WebTemplate.Core.Entities;
using WebTemplate.Core.Interfaces;
using WebTemplate.Core.Configuration;

namespace WebTemplate.Core.Services
{
    /// <summary>
    /// Authentication service implementation
    /// Handles user authentication, registration, and token management
    /// NO hard-coded values - all configuration from settings!
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ITokenService _tokenService;
        private readonly ILogger<AuthService> _logger;
        private readonly AuthSettings _authSettings;
        private readonly JwtSettings _jwtSettings;
        private readonly UserModuleFeatures _features;
        private readonly IUserTypeRepository _userTypeRepository;
        private readonly IEmailSender _emailSender;
        private readonly AppUrls _appUrls;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ITokenService tokenService,
            ILogger<AuthService> logger,
            IOptions<AuthSettings> authSettings,
            IOptions<JwtSettings> jwtSettings,
            IOptions<UserModuleFeatures> features,
            IUserTypeRepository userTypeRepository,
            IEmailSender emailSender,
            IOptions<AppUrls> appUrls)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
            _logger = logger;
            _authSettings = authSettings.Value;
            _jwtSettings = jwtSettings.Value;
            _features = features.Value;
            _userTypeRepository = userTypeRepository;
            _emailSender = emailSender;
            _appUrls = appUrls.Value;
        }

        private static List<string> ParsePermissions(string? permissionsJson)
        {
            if (string.IsNullOrWhiteSpace(permissionsJson)) return new List<string>();
            try
            {
                var list = JsonSerializer.Deserialize<List<string>>(permissionsJson);
                return list ?? new List<string>();
            }
            catch
            {
                return new List<string>();
            }
        }

        private static UserTypeDto MapUserType(UserType ut)
        {
            return new UserTypeDto
            {
                Id = ut.Id,
                Name = ut.Name,
                Description = ut.Description,
                IsActive = ut.IsActive,
                Permissions = ParsePermissions(ut.Permissions)
            };
        }

        private async Task<List<string>> GetEffectivePermissionsAsync(ApplicationUser user)
        {
            if (!_features.IncludeUserTypePermissionsInResponses)
                return new List<string>();

            var ut = await _userTypeRepository.GetByIdAsync(user.UserTypeId);
            return ut == null ? new List<string>() : ParsePermissions(ut.Permissions);
        }

        public async Task<ApiResponseDto<AuthResponseDto>> LoginAsync(LoginDto loginDto)
        {
            try
            {
                // Find user by email
                var user = await _userManager.FindByEmailAsync(loginDto.Email);
                if (user == null || !user.IsActive || user.IsDeleted)
                {
                    _logger.LogWarning("Login attempt failed for email: {Email}", loginDto.Email);
                    return new ApiResponseDto<AuthResponseDto>
                    {
                        Success = false,
                        Message = "Invalid email or password",
                        Data = null
                    };
                }

                // Check password
                var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, lockoutOnFailure: true);
                if (!result.Succeeded)
                {
                    _logger.LogWarning("Password check failed for user: {UserId}", user.Id);
                    return new ApiResponseDto<AuthResponseDto>
                    {
                        Success = false,
                        Message = result.IsLockedOut ? "Account is locked" : "Invalid email or password",
                        Data = null
                    };
                }

                // Update last login
                user.LastLoginAt = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);

                // Generate tokens
                var roles = await _userManager.GetRolesAsync(user);
                var accessToken = await _tokenService.GenerateAccessTokenAsync(user.Id, user.Email!, roles);
                var refreshToken = await _tokenService.GenerateRefreshTokenAsync(user.Id, loginDto.DeviceId);

                var permissions = await GetEffectivePermissionsAsync(user);
                UserTypeDto? userTypeDto = null;

                if (_features.IncludeUserTypePermissionsInResponses)
                {
                    var userType = await _userTypeRepository.GetByIdAsync(user.UserTypeId);
                    if (userType != null)
                    {
                        userTypeDto = MapUserType(userType);
                    }
                }

                var authResponse = new AuthResponseDto
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    AccessTokenExpiration = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpiryMinutes),
                    RefreshTokenExpiration = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryDays),
                    TokenType = "Bearer",
                    User = new UserProfileDto
                    {
                        Id = user.Id,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        FullName = $"{user.FirstName} {user.LastName}",
                        Email = user.Email!,
                        PhoneNumber = user.PhoneNumber,
                        IsActive = user.IsActive,
                        CreatedAt = user.CreatedAt,
                        LastLoginAt = user.LastLoginAt,
                        UserType = userTypeDto ?? new UserTypeDto { Id = user.UserTypeId },
                        Roles = roles.ToList()
                    },
                    Permissions = permissions,
                    AdditionalData = new Dictionary<string, object>
                    {
                        { "deviceId", loginDto.DeviceId ?? string.Empty },
                        { "deviceName", loginDto.DeviceName ?? string.Empty }
                    }
                };

                _logger.LogInformation("User {UserId} logged in successfully", user.Id);
                return new ApiResponseDto<AuthResponseDto>
                {
                    Success = true,
                    Message = "Login successful",
                    Data = authResponse
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for {Email}", loginDto.Email);
                return new ApiResponseDto<AuthResponseDto>
                {
                    Success = false,
                    Message = "An error occurred during login",
                    Data = null
                };
            }
        }

        public async Task<ApiResponseDto<AuthResponseDto>> RegisterAsync(RegisterDto registerDto)
        {
            try
            {
                // Check if user already exists
                var existingUser = await _userManager.FindByEmailAsync(registerDto.Email);
                if (existingUser != null)
                {
                    return new ApiResponseDto<AuthResponseDto>
                    {
                        Success = false,
                        Message = "User with this email already exists",
                        Data = null
                    };
                }

                // Create new user - uses provided or default UserTypeId
                var user = new ApplicationUser
                {
                    UserName = registerDto.Email,
                    Email = registerDto.Email,
                    FirstName = registerDto.FirstName,
                    LastName = registerDto.LastName,
                    PhoneNumber = registerDto.PhoneNumber,
                    UserTypeId = registerDto.UserTypeId,
                    IsActive = true,
                    EmailConfirmed = !_authSettings.User.RequireConfirmedEmail,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await _userManager.CreateAsync(user, registerDto.Password);
                if (!result.Succeeded)
                {
                    _logger.LogWarning("User registration failed for {Email}: {Errors}", registerDto.Email, string.Join(", ", result.Errors.Select(e => e.Description)));
                    return new ApiResponseDto<AuthResponseDto>
                    {
                        Success = false,
                        Message = "Registration failed",
                        Errors = result.Errors.Select(e => e.Description).ToList(),
                        Data = null
                    };
                }

                // Add to default role
                await _userManager.AddToRoleAsync(user, "User");

                // If email confirmation is required, send email
                if (_authSettings.User.RequireConfirmedEmail)
                {
                    try
                    {
                        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                        var encodedToken = HttpUtility.UrlEncode(token);

                        // Build frontend confirm URL
                        var baseUrl = string.IsNullOrWhiteSpace(_appUrls.FrontendBaseUrl) ? "" : _appUrls.FrontendBaseUrl.TrimEnd('/');
                        var path = string.IsNullOrWhiteSpace(_appUrls.ConfirmEmailPath) ? "/confirm-email" : _appUrls.ConfirmEmailPath;
                        var confirmUrl = $"{baseUrl}{path}?userId={HttpUtility.UrlEncode(user.Id)}&token={encodedToken}";

                        var subject = "Confirm your email";
                        var body = $"<p>Hello {System.Net.WebUtility.HtmlEncode(user.FirstName)},</p>" +
                                   $"<p>Thanks for registering. Please confirm your email by clicking the link below:</p>" +
                                   $"<p><a href=\"{confirmUrl}\">Confirm Email</a></p>" +
                                   $"<p>If you did not create this account, please ignore this email.</p>";

                        await _emailSender.SendAsync(user.Email!, subject, body);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to send confirmation email to {Email}", user.Email);
                        // Do not fail registration due to email issues
                    }

                    _logger.LogInformation("User {UserId} registered, confirmation required", user.Id);
                    return new ApiResponseDto<AuthResponseDto>
                    {
                        Success = true,
                        Message = "Registration successful. Please check your email to confirm your account.",
                        Data = null
                    };
                }

                // If email confirmation not required, auto-login
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    var accessToken = await _tokenService.GenerateAccessTokenAsync(user.Id, user.Email, roles);
                    var refreshToken = await _tokenService.GenerateRefreshTokenAsync(user.Id);

                    var permissions = await GetEffectivePermissionsAsync(user);

                    UserTypeDto? userTypeDto = null;
                    if (_features.IncludeUserTypePermissionsInResponses)
                    {
                        var userType = await _userTypeRepository.GetByIdAsync(user.UserTypeId);
                        if (userType != null)
                        {
                            userTypeDto = MapUserType(userType);
                        }
                    }

                    var authResponse = new AuthResponseDto
                    {
                        AccessToken = accessToken,
                        RefreshToken = refreshToken,
                        AccessTokenExpiration = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpiryMinutes),
                        RefreshTokenExpiration = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryDays),
                        TokenType = "Bearer",
                        User = new UserProfileDto
                        {
                            Id = user.Id,
                            FirstName = user.FirstName,
                            LastName = user.LastName,
                            FullName = $"{user.FirstName} {user.LastName}",
                            Email = user.Email,
                            PhoneNumber = user.PhoneNumber,
                            IsActive = user.IsActive,
                            CreatedAt = user.CreatedAt,
                            UserType = userTypeDto ?? new UserTypeDto { Id = user.UserTypeId },
                            Roles = roles.ToList()
                        },
                        Permissions = permissions,
                        AdditionalData = new Dictionary<string, object>()
                    };

                    _logger.LogInformation("User {UserId} registered and logged in successfully", user.Id);
                    return new ApiResponseDto<AuthResponseDto>
                    {
                        Success = true,
                        Message = "Registration successful",
                        Data = authResponse
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration for {Email}", registerDto.Email);
                return new ApiResponseDto<AuthResponseDto>
                {
                    Success = false,
                    Message = "An error occurred during registration",
                    Data = null
                };
            }
        }

        public async Task<ApiResponseDto<AuthResponseDto>> RefreshTokenAsync(RefreshTokenDto refreshTokenDto)
        {
            try
            {
                var userId = await _tokenService.ValidateRefreshTokenAsync(refreshTokenDto.RefreshToken);
                if (string.IsNullOrEmpty(userId))
                {
                    return new ApiResponseDto<AuthResponseDto>
                    {
                        Success = false,
                        Message = "Invalid refresh token",
                        Data = null
                    };
                }

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null || !user.IsActive || user.IsDeleted)
                {
                    return new ApiResponseDto<AuthResponseDto>
                    {
                        Success = false,
                        Message = "User not found or inactive",
                        Data = null
                    };
                }

                // Generate new tokens
                var roles = await _userManager.GetRolesAsync(user);
                var accessToken = await _tokenService.GenerateAccessTokenAsync(user.Id, user.Email!, roles);
                var refreshToken = await _tokenService.GenerateRefreshTokenAsync(user.Id);

                var permissions = await GetEffectivePermissionsAsync(user);

                UserTypeDto? userTypeDto = null;
                if (_features.IncludeUserTypePermissionsInResponses)
                {
                    var userType = await _userTypeRepository.GetByIdAsync(user.UserTypeId);
                    if (userType != null)
                    {
                        userTypeDto = MapUserType(userType);
                    }
                }

                var authResponse = new AuthResponseDto
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    AccessTokenExpiration = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpiryMinutes),
                    RefreshTokenExpiration = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryDays),
                    TokenType = "Bearer",
                    User = new UserProfileDto
                    {
                        Id = user.Id,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        FullName = $"{user.FirstName} {user.LastName}",
                        Email = user.Email!,
                        PhoneNumber = user.PhoneNumber,
                        IsActive = user.IsActive,
                        CreatedAt = user.CreatedAt,
                        LastLoginAt = user.LastLoginAt,
                        UserType = userTypeDto ?? new UserTypeDto { Id = user.UserTypeId },
                        Roles = roles.ToList()
                    },
                    Permissions = permissions,
                    AdditionalData = new Dictionary<string, object>()
                };

                return new ApiResponseDto<AuthResponseDto>
                {
                    Success = true,
                    Message = "Token refreshed successfully",
                    Data = authResponse
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token refresh");
                return new ApiResponseDto<AuthResponseDto>
                {
                    Success = false,
                    Message = "An error occurred during token refresh",
                    Data = null
                };
            }
        }

        public async Task<ApiResponseDto<bool>> LogoutAsync(LogoutDto logoutDto)
        {
            try
            {
                if (!string.IsNullOrEmpty(logoutDto.RefreshToken))
                {
                    // Validate the refresh token to get the user id
                    var userId = await _tokenService.ValidateRefreshTokenAsync(logoutDto.RefreshToken);
                    if (string.IsNullOrEmpty(userId))
                    {
                        return new ApiResponseDto<bool>
                        {
                            Success = false,
                            Message = "Invalid refresh token",
                            Data = false
                        };
                    }

                    bool revoked;
                    if (logoutDto.LogoutFromAllDevices)
                    {
                        revoked = await _tokenService.RevokeAllRefreshTokensAsync(userId);
                    }
                    else
                    {
                        revoked = await _tokenService.RevokeRefreshTokenAsync(logoutDto.RefreshToken);
                    }

                    if (!revoked)
                    {
                        return new ApiResponseDto<bool>
                        {
                            Success = false,
                            Message = "Failed to revoke refresh token(s)",
                            Data = false
                        };
                    }

                    _logger.LogInformation("User {UserId} logged out successfully (all devices: {All})", userId, logoutDto.LogoutFromAllDevices);
                }

                return new ApiResponseDto<bool>
                {
                    Success = true,
                    Message = "Logout successful",
                    Data = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                return new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = "An error occurred during logout",
                    Data = false
                };
            }
        }

        public async Task<ApiResponseDto<bool>> ConfirmEmailAsync(ConfirmEmailDto confirmEmailDto)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(confirmEmailDto.UserId);
                if (user == null)
                {
                    return new ApiResponseDto<bool> { Success = false, Message = "User not found", Data = false };
                }

                if (user.EmailConfirmed)
                {
                    return new ApiResponseDto<bool> { Success = true, Message = "Email already confirmed", Data = true };
                }

                var result = await _userManager.ConfirmEmailAsync(user, confirmEmailDto.Token);
                if (!result.Succeeded)
                {
                    return new ApiResponseDto<bool>
                    {
                        Success = false,
                        Message = "Email confirmation failed",
                        Errors = result.Errors.Select(e => e.Description).ToList(),
                        Data = false
                    };
                }

                return new ApiResponseDto<bool> { Success = true, Message = "Email confirmed", Data = true };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during email confirmation for user {UserId}", confirmEmailDto.UserId);
                return new ApiResponseDto<bool> { Success = false, Message = "An error occurred during email confirmation", Data = false };
            }
        }

        public async Task<ApiResponseDto<bool>> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(forgotPasswordDto.Email);
                if (user == null)
                {
                    // Do not reveal user existence
                    return new ApiResponseDto<bool> { Success = true, Message = "If an account exists, a reset email will be sent", Data = true };
                }

                var token = await _userManager.GeneratePasswordResetTokenAsync(user);

                // NOTE: In a real setup, generate a reset link using frontend URL from config
                var subject = "Password Reset";
                var body = $"Use this token to reset your password: {System.Net.WebUtility.HtmlEncode(token)}";

                // Best effort; do not fail the flow on email errors
                try { await _emailSender.SendAsync(user.Email!, subject, body); } catch { /* swallow */ }

                return new ApiResponseDto<bool> { Success = true, Message = "Password reset instructions sent if the account exists", Data = true };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during forgot password for {Email}", forgotPasswordDto.Email);
                return new ApiResponseDto<bool> { Success = false, Message = "An error occurred during password reset request", Data = false };
            }
        }

        public async Task<ApiResponseDto<bool>> ResetPasswordAsync(ResetPasswordDto resetPasswordDto)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(resetPasswordDto.Email);
                if (user == null)
                {
                    // Do not reveal user existence
                    return new ApiResponseDto<bool> { Success = true, Message = "Password reset completed if the account exists", Data = true };
                }

                var result = await _userManager.ResetPasswordAsync(user, resetPasswordDto.Token, resetPasswordDto.NewPassword);
                if (!result.Succeeded)
                {
                    return new ApiResponseDto<bool>
                    {
                        Success = false,
                        Message = "Password reset failed",
                        Errors = result.Errors.Select(e => e.Description).ToList(),
                        Data = false
                    };
                }

                return new ApiResponseDto<bool> { Success = true, Message = "Password has been reset", Data = true };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during password reset for {Email}", resetPasswordDto.Email);
                return new ApiResponseDto<bool> { Success = false, Message = "An error occurred during password reset", Data = false };
            }
        }

        public async Task<ApiResponseDto<bool>> ChangePasswordAsync(string userId, ChangePasswordDto changePasswordDto)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return new ApiResponseDto<bool> { Success = false, Message = "User not found", Data = false };
                }

                var result = await _userManager.ChangePasswordAsync(user, changePasswordDto.CurrentPassword, changePasswordDto.NewPassword);
                if (!result.Succeeded)
                {
                    return new ApiResponseDto<bool>
                    {
                        Success = false,
                        Message = "Change password failed",
                        Errors = result.Errors.Select(e => e.Description).ToList(),
                        Data = false
                    };
                }

                return new ApiResponseDto<bool> { Success = true, Message = "Password changed", Data = true };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password for user {UserId}", userId);
                return new ApiResponseDto<bool> { Success = false, Message = "An error occurred while changing password", Data = false };
            }
        }

        public async Task<ApiResponseDto<AuthStatusDto>> GetAuthStatusAsync(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return new ApiResponseDto<AuthStatusDto> { Success = true, Message = "User not authenticated", Data = new AuthStatusDto { IsAuthenticated = false } };
                }

                var roles = await _userManager.GetRolesAsync(user);

                // Include permissions and user type details when enabled
                var permissions = await GetEffectivePermissionsAsync(user);
                UserTypeDto? userTypeDto = null;
                if (_features.IncludeUserTypePermissionsInResponses)
                {
                    var userType = await _userTypeRepository.GetByIdAsync(user.UserTypeId);
                    if (userType != null)
                    {
                        userTypeDto = MapUserType(userType);
                    }
                }

                var status = new AuthStatusDto
                {
                    IsAuthenticated = true,
                    EmailConfirmed = user.EmailConfirmed,
                    RequiresTwoFactor = await _userManager.GetTwoFactorEnabledAsync(user),
                    User = new UserProfileDto
                    {
                        Id = user.Id,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        FullName = $"{user.FirstName} {user.LastName}",
                        Email = user.Email ?? string.Empty,
                        PhoneNumber = user.PhoneNumber,
                        IsActive = user.IsActive,
                        CreatedAt = user.CreatedAt,
                        LastLoginAt = user.LastLoginAt,
                        UserType = userTypeDto ?? new UserTypeDto { Id = user.UserTypeId },
                        Roles = roles.ToList()
                    },
                    Permissions = permissions
                };

                return new ApiResponseDto<AuthStatusDto> { Success = true, Message = "Auth status retrieved", Data = status };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting auth status for user {UserId}", userId);
                return new ApiResponseDto<AuthStatusDto> { Success = false, Message = "An error occurred while getting auth status", Data = null };
            }
        }

        public async Task<ApiResponseDto<Dictionary<string, object>>> ValidateTokenAsync(string token)
        {
            try
            {
                var principal = await _tokenService.ValidateAccessTokenAsync(token);
                if (principal == null)
                {
                    return new ApiResponseDto<Dictionary<string, object>> { Success = false, Message = "Invalid token", Data = null };
                }

                var claimsDict = principal.Claims
                    .GroupBy(c => c.Type)
                    .ToDictionary(g => g.Key, g => (object)g.Select(c => c.Value).ToArray());

                return new ApiResponseDto<Dictionary<string, object>> { Success = true, Message = "Token is valid", Data = claimsDict };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating token");
                return new ApiResponseDto<Dictionary<string, object>> { Success = false, Message = "An error occurred during token validation", Data = null };
            }
        }

        public async Task<ApiResponseDto<bool>> RevokeAllTokensAsync(string userId)
        {
            try
            {
                var result = await _tokenService.RevokeAllRefreshTokensAsync(userId);
                return new ApiResponseDto<bool> { Success = result, Message = result ? "All tokens revoked" : "Failed to revoke tokens", Data = result };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking tokens for user {UserId}", userId);
                return new ApiResponseDto<bool> { Success = false, Message = "An error occurred while revoking tokens", Data = false };
            }
        }
    }
}
