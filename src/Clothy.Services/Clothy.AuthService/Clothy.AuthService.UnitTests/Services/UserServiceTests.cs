using System.Net;
using System.Security.Claims;
using Clothy.AuthService.BLL.Config;
using Clothy.AuthService.BLL.DTOs.Users;
using Clothy.AuthService.BLL.Services;
using Clothy.AuthService.BLL.Services.Interfaces;
using Clothy.AuthService.UnitTests.Helpers;
using Clothy.Shared.Helpers.CloudinaryConfig.ImageService;
using Clothy.Shared.Helpers.JWT;
using FluentAssertions;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Clothy.AuthService.UnitTests.Services;

public class UserServiceTests
{
    private Mock<IUserClaimsExtractor> claimsExtractorMock;
    private Mock<IKeycloakUserHelper> keycloakUserHelperMock;
    private Mock<IImageService> imageServiceMock;
    private Mock<IPublishEndpoint> publishEndpointMock;
    private IOptions<KeycloakSettings> keycloakSettings;

    private const string FAKE_USER_ID = "11111111-1111-1111-1111-111111111111";
    private const string FAKE_EMAIL = "test@test.com";
    private const string FAKE_ADMIN_TOKEN = "fake-admin-token";

    public UserServiceTests()
    {
        claimsExtractorMock = new Mock<IUserClaimsExtractor>();
        keycloakUserHelperMock = new Mock<IKeycloakUserHelper>();
        imageServiceMock = new Mock<IImageService>();
        publishEndpointMock = new Mock<IPublishEndpoint>();

        keycloakSettings = Options.Create(new KeycloakSettings
        {
            Url = "http://localhost:8080",
            Realm = "clothy",
            ClientId = "clothy-client",
            ClientSecret = "secret"
        });

        claimsExtractorMock
            .Setup(x => x.GetUserId(It.IsAny<ClaimsPrincipal>()))
            .Returns(Guid.Parse(FAKE_USER_ID));
    }

    private UserService CreateService(HttpClient httpClient)
    {
        return new UserService(
            claimsExtractorMock.Object,
            httpClient,
            imageServiceMock.Object,
            NullLogger<UserService>.Instance,
            keycloakSettings,
            publishEndpointMock.Object,
            keycloakUserHelperMock.Object);
    }

    [Fact]
    public async Task GetCurrentUserAsync_ShouldReturnCurrentUser()
    {
        UserReadDTO fakeUser = BuildFakeUserReadDto();
        ClaimsPrincipal claimsPrincipal = BuildFakeClaimsPrincipal();

        HttpClient httpClient = MockHttpHelper.CreateHttpClientWithResponse(
            BuildAdminTokenResponse(),
            HttpStatusCode.OK);

        keycloakUserHelperMock
            .Setup(x => x.GetUserByIdAsync(FAKE_USER_ID, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(fakeUser);

        UserService service = CreateService(httpClient);

        UserReadDTO result = await service.GetCurrentUserAsync(claimsPrincipal);

        result.Should().NotBeNull();
        result.Id.Should().Be(Guid.Parse(FAKE_USER_ID));
        result.Email.Should().Be(FAKE_EMAIL);
    }

    [Fact]
    public async Task GetCurrentUserAsync_ShouldCallKeycloakWithCorrectUserId()
    {
        ClaimsPrincipal claimsPrincipal = BuildFakeClaimsPrincipal();

        HttpClient httpClient = MockHttpHelper.CreateHttpClientWithResponse(
            BuildAdminTokenResponse(),
            HttpStatusCode.OK);

        keycloakUserHelperMock
            .Setup(x => x.GetUserByIdAsync(FAKE_USER_ID, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildFakeUserReadDto());

        UserService service = CreateService(httpClient);

        await service.GetCurrentUserAsync(claimsPrincipal);

        keycloakUserHelperMock.Verify(
            x => x.GetUserByIdAsync(FAKE_USER_ID, It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateUserAsync_WhenKeycloakFails_ShouldThrowException()
    {
        ClaimsPrincipal claimsPrincipal = BuildFakeClaimsPrincipal();

        HttpClient httpClient = MockHttpHelper.CreateHttpClientWithSequence(
            (BuildAdminTokenResponse(), HttpStatusCode.OK),
            (new { error = "server error" }, HttpStatusCode.InternalServerError)
        );

        keycloakUserHelperMock
            .Setup(x => x.GetUserByIdAsync(FAKE_USER_ID, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildFakeUserReadDto());

        UserService service = CreateService(httpClient);

        UserUpdateDTO updateDto = new UserUpdateDTO
        {
            FirstName = "Name",
            LastName = "Last",
            PhoneNumber = "+380991112233"
        };

        var act = async () => await service.UpdateUserAsync(updateDto, claimsPrincipal);

        await act.Should().ThrowAsync<Exception>().WithMessage("*Failed to update user*");
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

    private static object BuildAdminTokenResponse() => new
    {
        access_token = FAKE_ADMIN_TOKEN,
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