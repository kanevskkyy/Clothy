using Clothy.ReviewService.Application.Features.Questions.Commands.CreateQuestion;
using Clothy.ReviewService.Domain.Entities;
using Clothy.ReviewService.Domain.Interfaces;
using Clothy.ReviewService.gRPC.Client.Services.Interfaces;
using Clothy.Shared.Helpers.Exceptions;
using FluentAssertions;
using Moq;
using System.Diagnostics.Metrics;
using Xunit;

namespace Clothy.ReviewService.UnitTests.Features.Questions.Commands;

public class CreateQuestionCommandHandlerTests
{
    private Mock<IQuestionRepository> repositoryMock = new();
    private Mock<IClotheItemIdValidatorGrpcClient> clotheValidatorMock = new();
    private CreateQuestionCommandHandler handler;
 
    private static Guid userId = Guid.NewGuid();
    private static Guid clotheId = Guid.NewGuid();
 
    public CreateQuestionCommandHandlerTests()
    {
        Meter meter = new Meter("Clothy.ReviewService.Questions.Tests");
        handler = new CreateQuestionCommandHandler(
            repositoryMock.Object,
            clotheValidatorMock.Object,
            meter
        );
    }
 
    private CreateQuestionCommand BuildCommand(Guid? userId = null, Guid? clotheId = null) =>
        new(
            clotheId ?? CreateQuestionCommandHandlerTests.clotheId,
            userId ?? CreateQuestionCommandHandlerTests.userId,
            "John", "Doe",
            "https://example.com/photo.jpg",
            "Does this come in blue?"
        );
 
    private void SetupClotheValidatorValid(Guid clotheId) =>
        clotheValidatorMock
            .Setup(c => c.ValidateClotheItemIdAsync(It.Is<ClotheItemIdToValidate>(x => x.ClotheId == clotheId.ToString())))
            .ReturnsAsync(new ClotheItemResponse
            {
                IsValid = true,
                ClotheName = "Test Hoodie",
                ClothePhotoUrl = "https://example.com/hoodie.jpg"
            });
 
    private void SetupClotheValidatorInvalid(Guid clotheId) =>
        clotheValidatorMock
            .Setup(c => c.ValidateClotheItemIdAsync(It.Is<ClotheItemIdToValidate>(x => x.ClotheId == clotheId.ToString())))
            .ReturnsAsync(new ClotheItemResponse { IsValid = false, ErrorMessage = "Not found" });
 
    [Fact]
    public async Task Handle_ValidRequest_CreatesAndReturnsQuestion()
    {
        SetupClotheValidatorValid(clotheId);
 
        Question result = await handler.Handle(BuildCommand(), default);
 
        result.Should().NotBeNull();
        result.User.UserId.Should().Be(userId);
        result.ClotheInfo.ClotheItemId.Should().Be(clotheId);
        result.QuestionText.Should().Be("Does this come in blue?");
        repositoryMock.Verify(r => r.AddAsync(It.IsAny<Question>(), default), Times.Once);
    }
 
    [Fact]
    public async Task Handle_InvalidClotheId_ThrowsValidationFailedException()
    {
        SetupClotheValidatorInvalid(clotheId);
 
        var act = () => handler.Handle(BuildCommand(), default);
 
        await act.Should().ThrowAsync<ValidationFailedException>();
        repositoryMock.Verify(r => r.AddAsync(It.IsAny<Question>(), default), Times.Never);
    }
 
    [Fact]
    public async Task Handle_ValidRequest_PopulatesClotheInfoFromGrpcResponse()
    {
        SetupClotheValidatorValid(clotheId);
 
        Question result = await handler.Handle(BuildCommand(), default);
 
        result.ClotheInfo.ClotheName.Should().Be("Test Hoodie");
        result.ClotheInfo.ClothePhotoURL.Should().Be("https://example.com/hoodie.jpg");
    }
}