using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebTemplate.Core.DTOs.Auth;
using WebTemplate.Core.Interfaces;
using WebTemplate.Core.Configuration;
using Microsoft.Extensions.Options;

namespace WebTemplate.API.Controllers
{
    /// <summary>
    /// Authentication controller - handles all authentication endpoints
    /// Uses configurable response messages - NO hard-coded text!
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ITokenService _tokenService;
        private readonly ILogger<AuthController> _logger;
        private readonly UserModuleFeatures _features;
        private readonly IEmailSender _emailSender;
        private readonly ResponseMessages _messages;

        public AuthController(IAuthService authService, ITokenService tokenService, IOptions<UserModuleFeatures> features, IEmailSender emailSender, IOptions<ResponseMessages> messages, ILogger<AuthController> logger)
        {
            _authService = authService;
            _tokenService = tokenService;
            _features = features.Value;
            _emailSender = emailSender;
            _messages = messages.Value;
            _logger = logger;
        }

        /// <summary>
        /// User login endpoint
        /// </summary>
        /// <param name="loginDto">Login credentials</param>
        /// <returns>Authentication response with tokens</returns>
        [HttpPost("login")]
        [ProducesResponseType(typeof(ApiResponseDto<AuthResponseDto>), 200)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 400)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 401)]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            if (!_features.EnableLogin) return NotFound();

            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponseDto<object>
                {
                    Success = false,
                    Message = _messages.Auth.InvalidInput,
                    Errors = ModelState.SelectMany(x => x.Value?.Errors.Select(e => e.ErrorMessage) ?? Array.Empty<string>()).ToList()
                });
            }

            try
            {
                var result = await _authService.LoginAsync(loginDto);
                return result.Success ? Ok(result) : Unauthorized(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for {Email}", loginDto.Email);
                return StatusCode(500, new ApiResponseDto<object>
                {
                    Success = false,
                    Message = _messages.Auth.LoginInternalError,
                    Errors = new List<string> { _messages.Auth.TryAgainLater }
                });
            }
        }

        /// <summary>
        /// User registration endpoint
        /// </summary>
        /// <param name="registerDto">Registration data</param>
        /// <returns>Authentication response with tokens or validation errors</returns>
        [HttpPost("register")]
        [ProducesResponseType(typeof(ApiResponseDto<AuthResponseDto>), 201)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 400)]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            if (!_features.EnableRegistration) return NotFound();

            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponseDto<object>
                {
                    Success = false,
                    Message = _messages.Auth.RegistrationInvalid,
                    Errors = ModelState.SelectMany(x => x.Value?.Errors.Select(e => e.ErrorMessage) ?? Array.Empty<string>()).ToList()
                });
            }

            try
            {
                var result = await _authService.RegisterAsync(registerDto);
                return result.Success ? Created("", result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration for {Email}", registerDto.Email);
                return StatusCode(500, new ApiResponseDto<object>
                {
                    Success = false,
                    Message = _messages.Auth.RegistrationInternalError,
                    Errors = new List<string> { _messages.Auth.TryAgainLater }
                });
            }
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto refreshTokenDto)
        {
            if (!_features.EnableRefreshToken) return NotFound();
            try
            {
                var result = await _authService.RefreshTokenAsync(refreshTokenDto);
                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new ApiResponseDto<object> { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token refresh");
                return StatusCode(500, new ApiResponseDto<object> { Success = false, Message = _messages.Auth.TokenRefreshError });
            }
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout([FromBody] LogoutDto logoutDto)
        {
            if (!_features.EnableLogout) return NotFound();
            if (logoutDto.LogoutFromAllDevices && !_features.EnableLogoutAllDevices) return NotFound();
            try
            {
                var result = await _authService.LogoutAsync(logoutDto);
                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                return StatusCode(500, new ApiResponseDto<object> { Success = false, Message = _messages.Auth.LogoutError });
            }
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto forgotPasswordDto)
        {
            if (!_features.EnableForgotPassword) return NotFound();
            try
            {
                var result = await _authService.ForgotPasswordAsync(forgotPasswordDto);
                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during forgot password for {Email}", forgotPasswordDto.Email);
                return StatusCode(500, new ApiResponseDto<object> { Success = false, Message = _messages.Auth.GenericError });
            }
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto)
        {
            if (!_features.EnableResetPassword) return NotFound();
            try
            {
                var result = await _authService.ResetPasswordAsync(resetPasswordDto);
                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during password reset for {Email}", resetPasswordDto.Email);
                return StatusCode(500, new ApiResponseDto<object> { Success = false, Message = _messages.Auth.PasswordResetError });
            }
        }

        [HttpPost("confirm-email")]
        public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailDto confirmEmailDto)
        {
            if (!_features.EnableConfirmEmail) return NotFound();
            try
            {
                var result = await _authService.ConfirmEmailAsync(confirmEmailDto);
                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during email confirmation for user {UserId}", confirmEmailDto.UserId);
                return StatusCode(500, new ApiResponseDto<object> { Success = false, Message = _messages.Auth.EmailConfirmationError });
            }
        }

        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
        {
            if (!_features.EnableChangePassword) return NotFound();
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new ApiResponseDto<object> { Success = false, Message = _messages.Auth.Unauthorized });
                }

                var result = await _authService.ChangePasswordAsync(userId, changePasswordDto);
                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during change password");
                return StatusCode(500, new ApiResponseDto<object> { Success = false, Message = _messages.Auth.ChangePasswordError });
            }
        }

        [HttpGet("status")]
        [Authorize]
        public async Task<IActionResult> Status()
        {
            // status should be always available for authenticated users
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new ApiResponseDto<object> { Success = false, Message = _messages.Auth.Unauthorized });
                }

                var result = await _authService.GetAuthStatusAsync(userId);

                var authHeader = Request.Headers["Authorization"].ToString();
                if (result.Success && result.Data != null && !string.IsNullOrWhiteSpace(authHeader) && authHeader.StartsWith("Bearer "))
                {
                    var token = authHeader["Bearer ".Length..].Trim();
                    var exp = _tokenService.GetTokenExpiration(token);
                    result.Data.TokenExpiration = exp;
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting auth status");
                return StatusCode(500, new ApiResponseDto<object> { Success = false, Message = _messages.Auth.GenericError });
            }
        }
    }
}