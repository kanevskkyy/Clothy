using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Clothy.Shared.Events.UserEvents;
using Clothy.Shared.Helpers.CloudinaryConfig;
using Clothy.Shared.Helpers.Exceptions;
using Clothy.UserService.BLL.DTOs.RoleDTOs;
using Clothy.UserService.BLL.DTOs.UserDTOs;
using Clothy.UserService.BLL.Exceptions;
using Clothy.UserService.BLL.Services.Interfaces;
using Clothy.UserService.Domain.Entities;
using MassTransit;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Clothy.UserService.BLL.Services
{
    public class UserService : IUserService
    {
        private UserManager<ApplicationUser> userManager;
        private RoleManager<ApplicationRole> roleManager;
        private IMapper mapper;
        private IImageService imageService;
        private IPublishEndpoint publishEndpoint;
        private static string DEFAULT_PHOTO_URL = "https://res.cloudinary.com/dkdljnfja/image/upload/v1763818143/Profile_Avatar_cfazhc.png";

        public UserService(UserManager<ApplicationUser> userManager, 
            RoleManager<ApplicationRole> roleManager, 
            IMapper mapper, 
            IImageService imageService,
            IPublishEndpoint publishEndpoint)
        {
            this.publishEndpoint = publishEndpoint;
            this.imageService = imageService;
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.mapper = mapper;
        }

        public async Task<List<UserReadDTO>> GetUsersAsync(CancellationToken cancellationToken = default)
        {
            List<ApplicationUser> users = await userManager.Users.ToListAsync(cancellationToken);
            List<UserReadDTO> userDTOs = mapper.Map<List<UserReadDTO>>(users);

            foreach (UserReadDTO userReadDTO in userDTOs)
            {
                ApplicationUser user = users.First(p => p.Id == userReadDTO.Id);
                var roles = await userManager.GetRolesAsync(user);

                userReadDTO.Roles = roles
                    .Select(r => new RoleReadDTO { RoleName = r })
                    .ToList();
            }
            return userDTOs;
        }

        public async Task<UserReadDTO> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            ApplicationUser? user = await userManager.FindByIdAsync(userId.ToString());
            if (user == null) throw new NotFoundException($"User with ID: {userId} not found");

            UserReadDTO userReadDTO = mapper.Map<UserReadDTO>(user);
            var roles = await userManager.GetRolesAsync(user);

            userReadDTO.Roles = roles
                .Select(role => new RoleReadDTO { RoleName = role })
                .ToList();

            return userReadDTO;
        }

        public async Task<UserReadDTO> UpdateUserByIdAsync(UserUpdateDTO userUpdateDTO, ClaimsPrincipal currentUser, CancellationToken cancellationToken = default)
        {
            string? currentUserId = currentUser.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (currentUserId == null) throw new UnauthorizedAccessException("User ID not found in token.");

            ApplicationUser? user = await userManager.FindByIdAsync(currentUserId);
            if (user == null) throw new NotFoundException($"User with ID: {currentUserId} not found");

            mapper.Map(userUpdateDTO, user);

            if (userUpdateDTO.Photo != null) user.PhotoUrl = await imageService.UploadAsync(userUpdateDTO.Photo, "users");

            var result = await userManager.UpdateAsync(user);
            if (!result.Succeeded) throw new IdentityOperationException("Failed to update user", result.Errors);

            UserUpdatedEvent userUpdatedEvent = new UserUpdatedEvent
            {
                UserId = Guid.Parse(currentUserId),
                FirstName = userUpdateDTO.FirstName,
                LastName = userUpdateDTO.LastName,
            };
            await publishEndpoint.Publish(userUpdatedEvent, cancellationToken);

            return mapper.Map<UserReadDTO>(user);
        }
        public async Task<IdentityResult> DeleteUserByIdAsync(Guid userId, ClaimsPrincipal claimsPrincipal, CancellationToken cancellationToken = default)
        {
            string? currentUserId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (currentUserId == null) throw new UnauthorizedAccessException("User ID not found in token.");

            if (!claimsPrincipal.IsInRole("Admin") && currentUserId != userId.ToString()) throw new ValidationFailedException("You can only update your own profile.");

            ApplicationUser? user = await userManager.FindByIdAsync(userId.ToString());
            if (user == null) return IdentityResult.Failed(new IdentityError { Description = $"User with ID {userId} not found." });

            if (user.PhotoUrl != DEFAULT_PHOTO_URL) await imageService.DeleteImageAsync(user.PhotoUrl);

            var result = await userManager.DeleteAsync(user);
            if (!result.Succeeded) throw new IdentityOperationException("Failed to update user", result.Errors);


            UserDeletedEvent userDeletedEvent = new UserDeletedEvent 
            {
                UserId = userId
            };
            await publishEndpoint.Publish(userDeletedEvent, cancellationToken);
            
            return result;
        }

        public async Task<IdentityResult> AddUserToRoleAsync(Guid userId, AddUserRoleDTO roleDTO, CancellationToken cancellationToken = default)
        {
            ApplicationUser? user = await userManager.FindByIdAsync(userId.ToString());
            if (user == null) throw new NotFoundException($"User with ID: {userId} not found");

            if (!await roleManager.RoleExistsAsync(roleDTO.Role)) return IdentityResult.Failed(new IdentityError { Description = "Role does not exist." });

            return await userManager.AddToRoleAsync(user, roleDTO.Role);
        }

        public async Task<IdentityResult> RemoveUserFromRoleAsync(Guid userId, RemoveUserRoleDTO roleDTO, CancellationToken cancellationToken = default)
        {
            ApplicationUser? user = await userManager.FindByIdAsync(userId.ToString());
            if (user == null) throw new NotFoundException($"User with ID: {userId} not found");

            if (!await roleManager.RoleExistsAsync(roleDTO.Role)) return IdentityResult.Failed(new IdentityError { Description = "Role does not exist." });

            return await userManager.RemoveFromRoleAsync(user, roleDTO.Role);
        }
    }
}
