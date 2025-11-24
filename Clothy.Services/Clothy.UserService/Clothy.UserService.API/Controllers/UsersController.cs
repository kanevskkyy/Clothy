using Clothy.UserService.BLL.DTOs.RoleDTOs;
using Clothy.UserService.BLL.DTOs.UserDTOs;
using Clothy.UserService.BLL.Services.Interfaces;
using k8s.KubeConfigModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Clothy.UserService.API.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private IUserService userService;
        private ILogger<UsersController> logger;

        public UsersController(IUserService userService, ILogger<UsersController> logger)
        {
            this.userService = userService;
            this.logger = logger;
        }

        /// <summary>
        /// Get all users (Admin only).
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUsers()
        {
            logger.LogInformation("Fetching all users.");
            List<UserReadDTO> users = await userService.GetUsersAsync();

            logger.LogInformation("Fetched {Count} users.", users.Count);
            return Ok(users);
        }

        /// <summary>
        /// Get a user by ID.
        /// </summary>
        /// <param name="id">User ID.</param>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(Guid id)
        {
            logger.LogInformation("Fetching user with ID: {Id}", id);
            UserReadDTO user = await userService.GetUserByIdAsync(id);

            logger.LogInformation("User with ID: {Id} fetched.", id);
            return Ok(user);
        }

        /// <summary>
        /// Update authenticated user's profile.
        /// </summary>
        /// <param name="userUpdateDTO">Update DTO.</param>
        [HttpPut("me")]
        [Authorize]
        public async Task<IActionResult> UpdateUser([FromForm] UserUpdateDTO userUpdateDTO)
        {
            logger.LogInformation("Updating user started");
            UserReadDTO updatedUser = await userService.UpdateUserByIdAsync(userUpdateDTO, User);

            logger.LogInformation("User with ID {Id} updated.", updatedUser.Id);
            return Ok(updatedUser);
        }

        /// <summary>
        /// Delete authenticated user's account (or Admin can delete any user).
        /// </summary>
        /// <param name="id">User ID.</param>
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            logger.LogInformation("Deleting user with ID: {Id}", id);
            var result = await userService.DeleteUserByIdAsync(id, User);
            if (!result.Succeeded)
            {
                logger.LogWarning("Failed to delete user with ID {Id}. Errors: {Errors}", id, result.Errors);
                return BadRequest(result.Errors);
            }
            logger.LogInformation("User with ID {Id} deleted.", id);
            return NoContent();
        }

        /// <summary>
        /// Add a role to a user (Admin only).
        /// </summary>
        [HttpPost("{id}/roles")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddRoleToUser(Guid id, [FromBody] AddUserRoleDTO roleDTO)
        {
            logger.LogInformation("Adding role {Role} to user with ID: {Id}", roleDTO.Role, id);
            var result = await userService.AddUserToRoleAsync(id, roleDTO);
            if (!result.Succeeded)
            {
                logger.LogWarning("Failed to add role {Role} to user with ID {Id}. Errors: {Errors}", roleDTO.Role, id, result.Errors);
                return BadRequest(result.Errors);
            }
            logger.LogInformation("Role {Role} added to user with ID: {Id}", roleDTO.Role, id);
            return Ok();
        }

        /// <summary>
        /// Get current authenticated user's info.
        /// </summary>
        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            string? userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                logger.LogWarning("Unauthorized access attempt to /me endpoint.");
                return Unauthorized();
            }

            Guid userId = Guid.Parse(userIdClaim);
            logger.LogInformation("Fetching current user info for ID: {UserId}", userId);

            UserReadDTO user = await userService.GetUserByIdAsync(userId);
            logger.LogInformation("Fetched current user info for ID: {UserId}", userId);
            return Ok(user);
        }


        /// <summary>
        /// Remove a role from a user (Admin only).
        /// </summary>
        [HttpDelete("{id}/roles")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RemoveRoleFromUser(Guid id, [FromBody] RemoveUserRoleDTO roleDTO)
        {
            logger.LogInformation("Removing role {Role} from user with ID: {Id}", roleDTO.Role, id);
            var result = await userService.RemoveUserFromRoleAsync(id, roleDTO);
            if (!result.Succeeded)
            {
                logger.LogWarning("Failed to remove role {Role} from user with ID {Id}. Errors: {Errors}", roleDTO.Role, id, result.Errors);
                return BadRequest(result.Errors);
            }
            logger.LogInformation("Role {Role} removed from user with ID: {Id}", roleDTO.Role, id);
            return NoContent();
        }
    }
}
