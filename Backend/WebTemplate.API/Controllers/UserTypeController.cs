namespace WebTemplate.API.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using WebTemplate.Core.DTOs;
    using WebTemplate.Core.DTOs.Auth;
    using WebTemplate.Core.Interfaces;

    /// <summary>
    /// UserType management controller - handles user type CRUD operations
    /// All endpoints require Admin role
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    [Produces("application/json")]
    public class UserTypeController : ControllerBase
    {
        private readonly IUserTypeService _userTypeService;

        public UserTypeController(IUserTypeService userTypeService)
        {
            _userTypeService = userTypeService;
        }

        /// <summary>
        /// Get all user types
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponseDto<IEnumerable<UserTypeDto>>), 200)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 401)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 403)]
        public async Task<IActionResult> GetAllUserTypes()
        {
            var userTypes = await _userTypeService.GetAllUserTypesAsync();
            return Ok(new ApiResponseDto<IEnumerable<UserTypeDto>>
            {
                Success = true,
                Message = "User types retrieved successfully",
                Data = userTypes
            });
        }

        /// <summary>
        /// Get user type by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponseDto<UserTypeDto>), 200)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 401)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 403)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 404)]
        public async Task<IActionResult> GetUserTypeById(int id)
        {
            var userType = await _userTypeService.GetUserTypeByIdAsync(id);
            if (userType == null)
            {
                return NotFound(new ApiResponseDto<object>
                {
                    Success = false,
                    Message = "UserType not found"
                });
            }

            return Ok(new ApiResponseDto<UserTypeDto>
            {
                Success = true,
                Message = "UserType retrieved successfully",
                Data = userType
            });
        }

        /// <summary>
        /// Create new user type
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponseDto<int>), 201)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 400)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 401)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 403)]
        public async Task<IActionResult> CreateUserType([FromBody] CreateUserTypeDto createUserTypeDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponseDto<object>
                {
                    Success = false,
                    Message = "Invalid user type data",
                    Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
                });
            }

            try
            {
                var userTypeId = await _userTypeService.CreateUserTypeAsync(createUserTypeDto);
                return CreatedAtAction(nameof(GetUserTypeById), new { id = userTypeId }, new ApiResponseDto<int>
                {
                    Success = true,
                    Message = "UserType created successfully",
                    Data = userTypeId
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiResponseDto<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// Update user type
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 200)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 400)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 401)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 403)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 404)]
        public async Task<IActionResult> UpdateUserType(int id, [FromBody] CreateUserTypeDto updateUserTypeDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponseDto<object>
                {
                    Success = false,
                    Message = "Invalid user type data",
                    Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
                });
            }

            try
            {
                var result = await _userTypeService.UpdateUserTypeAsync(id, updateUserTypeDto);
                if (!result)
                {
                    return NotFound(new ApiResponseDto<object>
                    {
                        Success = false,
                        Message = "UserType not found"
                    });
                }

                return Ok(new ApiResponseDto<object>
                {
                    Success = true,
                    Message = "UserType updated successfully"
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiResponseDto<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// Delete user type
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 200)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 400)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 401)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 403)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 404)]
        public async Task<IActionResult> DeleteUserType(int id)
        {
            try
            {
                var result = await _userTypeService.DeleteUserTypeAsync(id);
                if (!result)
                {
                    return NotFound(new ApiResponseDto<object>
                    {
                        Success = false,
                        Message = "UserType not found"
                    });
                }

                return Ok(new ApiResponseDto<object>
                {
                    Success = true,
                    Message = "UserType deleted successfully"
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiResponseDto<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// Toggle user type active status
        /// </summary>
        [HttpPost("{id}/toggle-status")]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 200)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 401)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 403)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 404)]
        public async Task<IActionResult> ToggleUserTypeStatus(int id)
        {
            var result = await _userTypeService.ToggleUserTypeStatusAsync(id);
            if (!result)
            {
                return NotFound(new ApiResponseDto<object>
                {
                    Success = false,
                    Message = "UserType not found"
                });
            }

            return Ok(new ApiResponseDto<object>
            {
                Success = true,
                Message = "UserType status toggled successfully"
            });
        }
    }
}
