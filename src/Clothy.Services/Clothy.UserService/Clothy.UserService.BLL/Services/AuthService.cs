using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Clothy.Shared.Helpers.Exceptions;
using Clothy.UserService.BLL.DTOs.AuthDTOs;
using Clothy.UserService.BLL.DTOs.TokenDTOs;
using Clothy.UserService.BLL.Exceptions;
using Clothy.UserService.BLL.Services.Interfaces;
using Clothy.UserService.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Clothy.UserService.BLL.Services
{
    public class AuthService : IAuthService
    {
        private ITokenService tokenService;
        private UserManager<ApplicationUser> userManager;
        private SignInManager<ApplicationUser> signInManager;
        private RoleManager<ApplicationRole> roleManager;
        private IMapper mapper;

        public AuthService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ITokenService tokenService, IMapper mapper, RoleManager<ApplicationRole> roleManager)
        {
            this.mapper = mapper;
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.tokenService = tokenService;
            this.roleManager = roleManager;
        }

        public async Task<IdentityResult> ChangePasswordAsync(ChangePasswordDTO changePasswordDTO, ClaimsPrincipal userPrincipal, CancellationToken cancellationToken = default)
        {
            string? userIdClaim = userPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return IdentityResult.Failed(new IdentityError { Description = "User is not authenticated." });

            ApplicationUser? user = await userManager.FindByIdAsync(userIdClaim);
            if (user == null) return IdentityResult.Failed(new IdentityError { Description = $"User with ID {userIdClaim} not found." });

            var result = await userManager.ChangePasswordAsync(user, changePasswordDTO.OldPassword, changePasswordDTO.NewPassword);
            return result;
        }

        public async Task<TokenResponseDTO> LoginAsync(LoginDTO loginDTO, CancellationToken cancellationToken = default)
        {
            ApplicationUser? user = await userManager.FindByEmailAsync(loginDTO.Email);
            if (user == null) throw new NotFoundException("Invalid email or password");

            var result = await signInManager.CheckPasswordSignInAsync(user, loginDTO.Password, false);
            if (!result.Succeeded) throw new UnauthorizedAccessException("Invalid email or password");

            return await tokenService.GenerateTokensAsync(user, cancellationToken);
        }

        public async Task<TokenResponseDTO> RegisterUserAsync(RegisterDTO registerDTO, CancellationToken cancellationToken = default)
        {
            ApplicationUser? applicationUser = await userManager.FindByEmailAsync(registerDTO.Email);
            if (applicationUser != null) throw new AlreadyExistsException($"User with email: {registerDTO.Email} already exists");

            ApplicationUser user = mapper.Map<ApplicationUser>(registerDTO);
            var result = await userManager.CreateAsync(user, registerDTO.Password);
            if (!result.Succeeded) throw new IdentityOperationException("Failed to register user", result.Errors);

            bool alreadyExist = await roleManager.RoleExistsAsync("User");
            if (!alreadyExist)
            {
                await roleManager.CreateAsync(new ApplicationRole { Name = "User" });
            }

            await userManager.AddToRoleAsync(user, "User");

            TokenResponseDTO tokenResponseDTO = await tokenService.GenerateTokensAsync(user, cancellationToken);
            return tokenResponseDTO;
        }
    }
}
