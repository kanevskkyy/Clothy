using Clothy.ReviewService.Application.Features.Questions.Commands.AddAnswer;
using Clothy.ReviewService.Domain.Entities;
using Clothy.ReviewService.Domain.Interfaces;
using Clothy.ReviewService.UnitTests.Helpers;
using Clothy.Shared.Helpers.Exceptions;
using FluentAssertions;
using Moq;
using Xunit;

namespace Clothy.ReviewService.UnitTests.Features.Questions.Commands;

public class AddAnswerCommandHandlerTests
{
    private Mock<IQuestionRepository> repositoryMock = new();
    private AddAnswerWithQuestionIdCommandHandler handler;
 
    public AddAnswerCommandHandlerTests()
    {
        handler = new AddAnswerWithQuestionIdCommandHandler(repositoryMock.Object);
    }
    
    private AddAnswerWithQuestionIdCommand BuildCommand(string questionId, Guid userId) => new(questionId, userId, "Jane", "Doe", "https://example.com/photo.jpg", "Great answer!");

    [Fact]
    public async Task Handle_ValidRequest_AddsAndReturnsAnswer()
    {
        Question question = QuestionFakes.CreateQuestion();
        Guid answerUserId = Guid.NewGuid();
        
        repositoryMock.Setup(r => r.GetByIdAsync(question.Id, default)).ReturnsAsync(question);
        
        Answer result = await handler.Handle(BuildCommand(question.Id, answerUserId), default);
 
        result.Should().NotBeNull();
        result.AnswerText.Should().Be("Great answer!");
        repositoryMock.Verify(r => r.AddAnswerAsync(question.Id, It.IsAny<Answer>(), default), Times.Once);
    }

    [Fact]
    public async Task Handle_UserAnswersOwnQuestion_ThrowsValidationFailedException()
    {
        Question question = QuestionFakes.CreateQuestion();
        repositoryMock.Setup(r => r.GetByIdAsync(question.Id, default)).ReturnsAsync(question);
        
        var act = () => handler.Handle(BuildCommand(question.Id, question.User.UserId), default);
 
        await act.Should().ThrowAsync<ValidationFailedException>().WithMessage("*own question*");
        repositoryMock.Verify(r => r.AddAnswerAsync(It.IsAny<string>(), It.IsAny<Answer>(), default), Times.Never);
    }
    
    [Fact]
    public async Task Handle_QuestionNotFound_ThrowsNotFoundException()
    {
        repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<string>(), default)).ReturnsAsync((Question?)null);
 
        var act = () => handler.Handle(BuildCommand("nonexistent-id", Guid.NewGuid()), default);
 
        await act.Should().ThrowAsync<NotFoundException>();
    }
}