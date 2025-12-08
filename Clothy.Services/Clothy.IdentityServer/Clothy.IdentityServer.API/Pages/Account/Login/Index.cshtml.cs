using Clothy.IdentityServer.API.Data.Entities;
using Duende.IdentityServer;
using Duende.IdentityServer.Events;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Duende.IdentityServer.Stores;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace IdentityServerHost.Pages.Login;

[SecurityHeaders]
[AllowAnonymous]
public class Index : PageModel
{
    private IIdentityServerInteractionService interaction;
    private IEventService events;
    private IAuthenticationSchemeProvider schemeProvider;
    private IIdentityProviderStore identityProviderStore;
    private UserManager<ApplicationUser> userManager;
    private SignInManager<ApplicationUser> signInManager;

    public ViewModel View { get; set; } = default!;
    [BindProperty] public InputModel Input { get; set; } = default!;

    public Index(
        IIdentityServerInteractionService interaction,
        IAuthenticationSchemeProvider schemeProvider,
        IIdentityProviderStore identityProviderStore,
        IEventService events,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        this.interaction = interaction;
        this.schemeProvider = schemeProvider;
        this.identityProviderStore = identityProviderStore;
        this.events = events;
        this.userManager = userManager;
        this.signInManager = signInManager;
    }

    public async Task<IActionResult> OnGet(string? returnUrl)
    {
        await BuildModelAsync(returnUrl);

        if (View.IsExternalLoginOnly)
        {
            return RedirectToPage("/ExternalLogin/Challenge", new { scheme = View.ExternalLoginScheme, returnUrl });
        }

        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        var context = await interaction.GetAuthorizationContextAsync(Input.ReturnUrl);

        if (Input.Button != "login")
        {
            if (context != null)
            {
                await interaction.DenyAuthorizationAsync(context, AuthorizationError.AccessDenied);
                if (context.IsNativeClient())
                    return this.LoadingPage(Input.ReturnUrl);

                return Redirect(Input.ReturnUrl ?? "~/");
            }

            return Redirect("~/");
        }

        if (!ModelState.IsValid)
        {
            await BuildModelAsync(Input.ReturnUrl);
            return Page();
        }

        var user = await userManager.FindByNameAsync(Input.Username);
        if (user != null)
        {
            var result = await signInManager.CheckPasswordSignInAsync(user, Input.Password, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                await events.RaiseAsync(new UserLoginSuccessEvent(
                    user.UserName,
                    user.Id.ToString(),
                    user.UserName,
                    clientId: context?.Client.ClientId));

                AuthenticationProperties props = new AuthenticationProperties();
                if (LoginOptions.AllowRememberLogin && Input.RememberLogin)
                {
                    props.IsPersistent = true;
                    props.ExpiresUtc = DateTimeOffset.UtcNow.Add(LoginOptions.RememberMeLoginDuration);
                }

                List<Claim> claims = new List<Claim>
                {
                    new Claim("sub", user.Id.ToString()),
                    new Claim("name", user.UserName ?? ""),
                    new Claim("idp", IdentityServerConstants.LocalIdentityProvider),
                    new Claim("amr", "pwd"),
                    new Claim("auth_time", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString())
                };

                if (!string.IsNullOrEmpty(user.FirstName)) claims.Add(new Claim(ClaimTypes.GivenName, user.FirstName));
                if (!string.IsNullOrEmpty(user.LastName)) claims.Add(new Claim(ClaimTypes.Surname, user.LastName));
                if (!string.IsNullOrEmpty(user.Email)) claims.Add(new Claim(ClaimTypes.Email, user.Email));
                if (!string.IsNullOrEmpty(user.PhoneNumber)) claims.Add(new Claim("phone_number", user.PhoneNumber));
                if (!string.IsNullOrEmpty(user.PhotoUrl)) claims.Add(new Claim("photo_url", user.PhotoUrl));

                var roles = await userManager.GetRolesAsync(user);
                foreach (var role in roles)
                {
                    claims.Add(new Claim("role", role));
                }

                var isuser = new IdentityServerUser(user.Id.ToString())
                {
                    DisplayName = user.UserName,
                    AdditionalClaims = claims
                };

                await HttpContext.SignInAsync(isuser, props);

                if (context != null)
                {
                    if (context.IsNativeClient()) return this.LoadingPage(Input.ReturnUrl);

                    return Redirect(Input.ReturnUrl ?? "~/");
                }

                if (Url.IsLocalUrl(Input.ReturnUrl)) return Redirect(Input.ReturnUrl);
                if (string.IsNullOrEmpty(Input.ReturnUrl)) return Redirect("~/");

                throw new ArgumentException("Invalid return URL");
            }
        }

        const string error = "Invalid username or password";
        await events.RaiseAsync(new UserLoginFailureEvent(Input.Username, error, clientId: context?.Client.ClientId));
        ModelState.AddModelError(string.Empty, LoginOptions.InvalidCredentialsErrorMessage);

        await BuildModelAsync(Input.ReturnUrl);
        return Page();
    }


    private async Task BuildModelAsync(string? returnUrl)
    {
        Input = new InputModel { ReturnUrl = returnUrl };

        var context = await interaction.GetAuthorizationContextAsync(returnUrl);
        var schemes = await schemeProvider.GetAllSchemesAsync();
        var providers = schemes
            .Where(x => x.DisplayName != null)
            .Select(x => new ViewModel.ExternalProvider(x.Name, x.DisplayName!))
            .ToList();

        var dynamicSchemes = (await identityProviderStore.GetAllSchemeNamesAsync())
            .Where(x => x.Enabled)
            .Select(x => new ViewModel.ExternalProvider(x.Scheme, x.DisplayName ?? x.Scheme));

        providers.AddRange(dynamicSchemes);

        var allowLocal = true;
        var client = context?.Client;
        if (client != null)
        {
            allowLocal = client.EnableLocalLogin;
            if (client.IdentityProviderRestrictions?.Count > 0)
                providers = providers.Where(p => client.IdentityProviderRestrictions.Contains(p.AuthenticationScheme)).ToList();
        }

        View = new ViewModel
        {
            AllowRememberLogin = LoginOptions.AllowRememberLogin,
            EnableLocalLogin = allowLocal && LoginOptions.AllowLocalLogin,
            ExternalProviders = providers.ToArray()
        };
    }
}
