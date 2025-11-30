using Clothy.AuthService.BLL.DTOs.Users;
using Clothy.AuthService.BLL.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clothy.AuthService.API.Controllers
{
    /// <summary>
    /// Controller for user management
    /// </summary>
    [ApiController]
    [Route("api/users")]
    [Authorize]
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
        /// Get current user information
        /// </summary>
        /// <param name="cancellationToken">Operation cancellation token</param>
        /// <returns>Current user data</returns>
        /// <response code="200">Successfully retrieved user data</response>
        /// <response code="400">Error retrieving data</response>
        /// <response code="401">User not authorized</response>
        [HttpGet("me")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetCurrentUser(CancellationToken cancellationToken)
        {
            logger.LogInformation("Get current user request");
            var user = await userService.GetCurrentUserAsync(User, cancellationToken);
            return Ok(user);
        }

        /// <summary>
        /// Update current user data
        /// </summary>
        /// <param name="userUpdateDTO">Data to update (FirstName, LastName, PhoneNumber, Photo)</param>
        /// <param name="cancellationToken">Operation cancellation token</param>
        /// <returns>Updated user data</returns>
        /// <response code="200">User data successfully updated</response>
        /// <response code="400">Error updating data</response>
        /// <response code="401">User not authorized</response>
        [HttpPut("me")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateCurrentUser([FromForm] UserUpdateDTO userUpdateDTO, CancellationToken cancellationToken)
        {
            logger.LogInformation("Update current user request");
            var user = await userService.UpdateUserAsync(userUpdateDTO, User, cancellationToken);
            return Ok(user);
        }
    }
}