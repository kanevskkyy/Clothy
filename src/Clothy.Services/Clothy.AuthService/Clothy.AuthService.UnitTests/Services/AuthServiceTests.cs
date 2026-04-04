using System.Net;
using System.Security.Claims;
using AutoMapper;
using Clothy.AuthService.BLL.Config;
using Clothy.AuthService.BLL.DTOs.Auth;
using Clothy.AuthService.BLL.DTOs.Users;
using Clothy.AuthService.BLL.Services.Interfaces;
using Clothy.AuthService.UnitTests.Helpers;
using Clothy.Shared.Cache.Interfaces;
using Clothy.Shared.Helpers.Exceptions;
using Clothy.Shared.Helpers.JWT;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Clothy.AuthService.UnitTests.Services;

public class AuthServiceTests
{
    private Mock<IUserClaimsExtractor> claimsExtractorMock;
    private Mock<IKeycloakUserHelper> keycloakUserHelperMock;
    private Mock<IEntityCacheService> cacheServiceMock;
    private Mock<IMapper> mapperMock;
    private IOptions<KeycloakSettings> keycloakSettings;

    private const string FAKE_ADMIN_TOKEN = "fake-admin-token";
    private const string FAKE_ACCESS_TOKEN = "fake-access-token";
    private const string FAKE_REFRESH_TOKEN = "fake-refresh-token";
    private const string FAKE_USER_ID = "11111111-1111-1111-1111-111111111111";
    private const string FAKE_EMAIL = "test@test.com";

    public AuthServiceTests()
    {
        claimsExtractorMock = new Mock<IUserClaimsExtractor>();
        keycloakUserHelperMock = new Mock<IKeycloakUserHelper>();
        cacheServiceMock = new Mock<IEntityCacheService>();
        mapperMock = new Mock<IMapper>();

        keycloakSettings = Options.Create(new KeycloakSettings
        {
            Url = "http://localhost:8080",
            Realm = "clothy",
            ClientId = "clothy-client",
            ClientSecret = "secret"
        });

        cacheServiceMock
            .Setup(x => x.GetOrSetAsync<string>(
                It.IsAny<string>(),
                It.IsAny<Func<Task<string?>>>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<TimeSpan?>()))
            .Returns(async (string _, Func<Task<string?>> factory, TimeSpan? __, TimeSpan? ___) =>
                await factory());
    }

    private BLL.Services.AuthService CreateService(HttpClient httpClient)
    {
        return new BLL.Services.AuthService(
            httpClient,
            NullLogger<BLL.Services.AuthService>.Instance,
            keycloakSettings,
            claimsExtractorMock.Object,
            mapperMock.Object,
            keycloakUserHelperMock.Object,
            cacheServiceMock.Object);
    }

    [Fact]
    public async Task RegisterUserAsync_WhenKeycloakFails_ShouldThrowException()
    {
        HttpClient httpClient = MockHttpHelper.CreateHttpClientWithSequence(
            (BuildAdminTokenResponse(), HttpStatusCode.OK),
            (new { errorMessage = "User already exists" }, HttpStatusCode.Conflict)
        );

        BLL.Services.AuthService service = CreateService(httpClient);

        RegisterDTO dto = new RegisterDTO
        {
            Email = FAKE_EMAIL,
            Password = "Test1234!",
            FirstName = "Test",
            LastName = "User",
            PhoneNumber = "+380991234567"
        };

        var act = async () => await service.RegisterUserAsync(dto);
        
        await act.Should().ThrowAsync<Exception>().WithMessage("*Registration failed*");
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ShouldReturnTokensAndUser()
    {
        UserReadDTO fakeUser = BuildFakeUserReadDto();

        HttpClient httpClient = MockHttpHelper.CreateHttpClientWithSequence(
            (BuildUserTokenResponse(), HttpStatusCode.OK),  
            (BuildAdminTokenResponse(), HttpStatusCode.OK)  
        );

        keycloakUserHelperMock
            .Setup(x => x.GetUserByEmailAsync(FAKE_EMAIL, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(fakeUser);

        mapperMock
            .Setup(x => x.Map<TokenResponseDTO>(It.IsAny<object>()))
            .Returns(BuildFakeTokenResponse());

        BLL.Services.AuthService service = CreateService(httpClient);

        LoginResponseDTO result = await service.LoginAsync(new LoginDTO
        {
            Email = FAKE_EMAIL,
            Password = "Test1234!"
        });

        result.Should().NotBeNull();
        result.Tokens.AccessToken.Should().Be(FAKE_ACCESS_TOKEN);
        result.User.Email.Should().Be(FAKE_EMAIL);
    }

    [Fact]
    public async Task LoginAsync_WithInvalidCredentials_ShouldThrowValidationFailedException()
    {
        HttpClient httpClient = MockHttpHelper.CreateHttpClientWithResponse(
            new { error = "invalid_grant" },
            HttpStatusCode.Unauthorized);

        BLL.Services.AuthService service = CreateService(httpClient);

        var act = async () => await service.LoginAsync(new LoginDTO
        {
            Email = FAKE_EMAIL,
            Password = "WrongPassword"
        });

        await act.Should().ThrowAsync<ValidationFailedException>().WithMessage("*Invalid email or password*");
    }

    [Fact]
    public async Task RefreshTokenAsync_WithValidToken_ShouldReturnNewTokens()
    {
        HttpClient httpClient = MockHttpHelper.CreateHttpClientWithResponse(
            BuildUserTokenResponse(),
            HttpStatusCode.OK);

        mapperMock
            .Setup(x => x.Map<TokenResponseDTO>(It.IsAny<object>()))
            .Returns(BuildFakeTokenResponse());

        BLL.Services.AuthService service = CreateService(httpClient);
        
        TokenResponseDTO result = await service.RefreshTokenAsync(new RefreshTokenDTO
        {
            RefreshToken = FAKE_REFRESH_TOKEN
        });
        
        result.Should().NotBeNull();
        result.AccessToken.Should().Be(FAKE_ACCESS_TOKEN);
    }

    [Fact]
    public async Task RefreshTokenAsync_WithInvalidToken_ShouldThrowUnauthorizedException()
    {
        HttpClient httpClient = MockHttpHelper.CreateHttpClientWithResponse(
            new { error = "invalid_grant" },
            HttpStatusCode.BadRequest);

        BLL.Services.AuthService service = CreateService(httpClient);

        var act = async () => await service.RefreshTokenAsync(new RefreshTokenDTO
        {
            RefreshToken = "invalid-token"
        });

        await act.Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("*Invalid refresh token*");
    }
    
    [Fact]
    public async Task LogoutAsync_WithValidToken_ShouldCompleteSuccessfully()
    {
        HttpClient httpClient = MockHttpHelper.CreateHttpClientWithResponse(
            new { },
            HttpStatusCode.NoContent);

        BLL.Services.AuthService service = CreateService(httpClient);

        var act = async () => await service.LogoutAsync(FAKE_REFRESH_TOKEN);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task LogoutAsync_WhenKeycloakFails_ShouldStillCompleteWithoutException()
    {
        HttpClient httpClient = MockHttpHelper.CreateHttpClientWithResponse(
            new { error = "server error" },
            HttpStatusCode.InternalServerError);

        BLL.Services.AuthService service = CreateService(httpClient);

        var act = async () => await service.LogoutAsync(FAKE_REFRESH_TOKEN);

        await act.Should().NotThrowAsync();
    }
    
    [Fact]
    public async Task SendPasswordResetEmailAsync_WithValidEmail_ShouldCompleteSuccessfully()
    {
        HttpClient httpClient = MockHttpHelper.CreateHttpClientWithSequence(
            (BuildAdminTokenResponse(), HttpStatusCode.OK),
            (new { }, HttpStatusCode.OK)
        );

        keycloakUserHelperMock
            .Setup(x => x.GetUserIdByEmailAsync(FAKE_EMAIL, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(FAKE_USER_ID);

        BLL.Services.AuthService service = CreateService(httpClient);

        var act = async () => await service.SendPasswordResetEmailAsync(
            new ForgotPasswordDTO { Email = FAKE_EMAIL });

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task SendPasswordResetEmailAsync_WithNonExistentEmail_ShouldReturnWithoutException()
    {
        HttpClient httpClient = MockHttpHelper.CreateHttpClientWithResponse(
            BuildAdminTokenResponse(),
            HttpStatusCode.OK);

        keycloakUserHelperMock
            .Setup(x => x.GetUserIdByEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(string.Empty);

        BLL.Services.AuthService service = CreateService(httpClient);

        var act = async () => await service.SendPasswordResetEmailAsync(
            new ForgotPasswordDTO { Email = "nonexistent@test.com" });

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ResetPasswordAsync_WithCorrectCurrentPassword_ShouldCompleteSuccessfully()
    {
        ClaimsPrincipal claimsPrincipal = BuildFakeClaimsPrincipal();

        claimsExtractorMock
            .Setup(x => x.GetUserId(It.IsAny<ClaimsPrincipal>()))
            .Returns(Guid.Parse(FAKE_USER_ID));

        claimsExtractorMock
            .Setup(x => x.GetEmail(It.IsAny<ClaimsPrincipal>()))
            .Returns(FAKE_EMAIL);

        mapperMock
            .Setup(x => x.Map<TokenResponseDTO>(It.IsAny<object>()))
            .Returns(BuildFakeTokenResponse());

        keycloakUserHelperMock
            .Setup(x => x.GetUserByEmailAsync(FAKE_EMAIL, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildFakeUserReadDto());

        HttpClient httpClient = MockHttpHelper.CreateHttpClientWithSequence(
            (BuildUserTokenResponse(), HttpStatusCode.OK), 
            (BuildAdminTokenResponse(), HttpStatusCode.OK), 
            (BuildAdminTokenResponse(), HttpStatusCode.OK),  
            (new { }, HttpStatusCode.NoContent)             
        );

        BLL.Services.AuthService service = CreateService(httpClient);

        var act = async () => await service.ResetPasswordAsync(
            new ResetPasswordDTO
            {
                CurrentPassword = "OldPassword1!",
                NewPassword = "NewPassword1!"
            },
            claimsPrincipal);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ResetPasswordAsync_WithWrongCurrentPassword_ShouldThrowValidationFailedException()
    {
        ClaimsPrincipal claimsPrincipal = BuildFakeClaimsPrincipal();

        claimsExtractorMock
            .Setup(x => x.GetUserId(It.IsAny<ClaimsPrincipal>()))
            .Returns(Guid.Parse(FAKE_USER_ID));

        claimsExtractorMock
            .Setup(x => x.GetEmail(It.IsAny<ClaimsPrincipal>()))
            .Returns(FAKE_EMAIL);

        HttpClient httpClient = MockHttpHelper.CreateHttpClientWithResponse(
            new { error = "invalid_grant" },
            HttpStatusCode.Unauthorized);

        BLL.Services.AuthService service = CreateService(httpClient);

        var act = async () => await service.ResetPasswordAsync(
            new ResetPasswordDTO
            {
                CurrentPassword = "WrongPassword",
                NewPassword = "NewPassword1!"
            },
            claimsPrincipal);

        await act.Should().ThrowAsync<ValidationFailedException>().WithMessage("*Current password is incorrect*");
    }
    
    private static UserReadDTO BuildFakeUserReadDto() => new()
    {
        Id = Guid.Parse(FAKE_USER_ID),
        Email = FAKE_EMAIL,
        FirstName = "Test",
        LastName = "User",
        PhoneNumber = "+380991234567",
        PhotoUrl = "https://example.com/photo.png"
    };

    private static TokenResponseDTO BuildFakeTokenResponse() => new()
    {
        AccessToken = FAKE_ACCESS_TOKEN,
        RefreshToken = FAKE_REFRESH_TOKEN,
        ExpiresIn = 300
    };

    private static object BuildAdminTokenResponse() => new
    {
        access_token = FAKE_ADMIN_TOKEN,
        expires_in = 300,
        token_type = "Bearer"
    };

    private static object BuildUserTokenResponse() => new
    {
        access_token = FAKE_ACCESS_TOKEN,
        refresh_token = FAKE_REFRESH_TOKEN,
        expires_in = 300,
        token_type = "Bearer"
    };

    private static ClaimsPrincipal BuildFakeClaimsPrincipal() =>
        new(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, FAKE_USER_ID),
            new Claim(ClaimTypes.Email, FAKE_EMAIL)
        }));
}