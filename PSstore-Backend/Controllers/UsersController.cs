using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using PSstore.DTOs;
using PSstore.Interfaces;

namespace PSstore.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IEntitlementService _entitlementService;

        public UsersController(IUserService userService, IEntitlementService entitlementService)
        {
            _userService = userService;
            _entitlementService = entitlementService;
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<UserDTO>> GetUserById(Guid id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
                return NotFound(new { message = "User not found." });

            return Ok(user);
        }

        [HttpGet("email/{email}")]
        public async Task<ActionResult<UserDTO>> GetUserByEmail(string email)
        {
            var user = await _userService.GetUserByEmailAsync(email);
            if (user == null)
                return NotFound(new { message = "User not found." });

            return Ok(user);
        }

        [HttpPost]
        public async Task<ActionResult<UserDTO>> CreateUser([FromBody] CreateUserDTO createUserDTO)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var user = await _userService.CreateUserAsync(createUserDTO);
                return CreatedAtAction(nameof(GetUserById), new { id = user.UserId }, user);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<ActionResult<UserDTO>> UpdateUser(Guid id, [FromBody] UpdateUserDTO updateUserDTO)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userService.UpdateUserAsync(id, updateUserDTO);
            if (user == null)
                return NotFound(new { message = "User not found." });

            return Ok(user);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> SoftDeleteUser(Guid id)
        {
            var result = await _userService.SoftDeleteUserAsync(id);
            if (!result)
                return NotFound(new { message = "User not found." });

            return NoContent();
        }

        [HttpPost("{id}/restore")]
        public async Task<ActionResult> RestoreUser(Guid id)
        {
            var result = await _userService.RestoreUserAsync(id);
            if (!result)
                return NotFound(new { message = "User not found." });

            return Ok(new { message = "User restored successfully." });
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponseDTO>> Login([FromBody] LoginDTO loginDTO)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var loginResponse = await _userService.LoginAsync(loginDTO);
            if (loginResponse == null)
                return Unauthorized(new { message = "Invalid email or password." });

            return Ok(loginResponse);
        }

        [HttpPost("{id}/change-password")]
        public async Task<ActionResult> ChangePassword(Guid id, [FromBody] ChangePasswordDTO changePasswordDTO)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _userService.ChangePasswordAsync(id, changePasswordDTO.OldPassword, changePasswordDTO.NewPassword);
            if (!result)
                return BadRequest(new { message = "Failed to change password. Check your old password." });

            return Ok(new { message = "Password changed successfully." });
        }

        [HttpGet("{userId}/library")]
        public async Task<ActionResult<UserLibraryDTO>> GetUserLibrary(Guid userId)
        {
            var library = await _entitlementService.GetUserLibraryAsync(userId);
            return Ok(library);
        }

        [HttpGet("{userId}/games/{gameId}/access")]
        public async Task<ActionResult<GameAccessResultDTO>> CheckGameAccess(Guid userId, Guid gameId)
        {
            var result = await _entitlementService.CanUserAccessGameAsync(userId, gameId);
            return Ok(result);
        }
    }

    public class ChangePasswordDTO
    {
        public string OldPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
}
