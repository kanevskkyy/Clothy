using Clothy.ReviewService.Application.Features.Questions.Commands.UpdateAnswer;
using Clothy.ReviewService.Domain.Entities;
using Clothy.ReviewService.Domain.Interfaces;
using Clothy.ReviewService.UnitTests.Helpers;
using Clothy.Shared.Helpers.Exceptions;
using FluentAssertions;
using Moq;
using Xunit;

namespace Clothy.ReviewService.UnitTests.Features.Questions.Commands;

public class UpdateAnswerCommandHandlerTests
{
    private Mock<IQuestionRepository> repositoryMock = new();
    private UpdateAnswerWithIdsCommandHandler handler;
 
    public UpdateAnswerCommandHandlerTests()
    {
        handler = new UpdateAnswerWithIdsCommandHandler(repositoryMock.Object);
    }
 
    [Fact]
    public async Task Handle_AnswerOwnerUpdates_UpdatesSuccessfully()
    {
        var (question, answer) = QuestionFakes.CreateQuestionWithAnswer();
        repositoryMock.Setup(r => r.GetByIdAsync(question.Id, default)).ReturnsAsync(question);
 
        UpdateAnswerWithIdsCommand command = new UpdateAnswerWithIdsCommand(question.Id, answer.Id, "Updated answer text", answer.User.UserId);
        await handler.Handle(command, default);
 
        answer.AnswerText.Should().Be("Updated answer text");
        repositoryMock.Verify(r => r.UpdateAnswerAsync(question.Id, answer, default), Times.Once);
    }
 
    [Fact]
    public async Task Handle_OtherUserUpdates_ThrowsForbiddenException()
    {
        var (question, answer) = QuestionFakes.CreateQuestionWithAnswer();
        repositoryMock.Setup(r => r.GetByIdAsync(question.Id, default)).ReturnsAsync(question);
 
        UpdateAnswerWithIdsCommand command = new UpdateAnswerWithIdsCommand(question.Id, answer.Id, "Hacked text", Guid.NewGuid());
        var act = () => handler.Handle(command, default);
 
        await act.Should().ThrowAsync<ForbiddenException>();
        repositoryMock.Verify(r => r.UpdateAnswerAsync(It.IsAny<string>(), It.IsAny<Answer>(), default), Times.Never);
    }
 
    [Fact]
    public async Task Handle_QuestionNotFound_ThrowsNotFoundException()
    {
        repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<string>(), default)).ReturnsAsync((Question?)null);
 
        UpdateAnswerWithIdsCommand command = new UpdateAnswerWithIdsCommand("nonexistent-q", "some-answer", "text", Guid.NewGuid());
        var act = () => handler.Handle(command, default);
 
        await act.Should().ThrowAsync<NotFoundException>();
    }
 
    [Fact]
    public async Task Handle_AnswerNotFound_ThrowsNotFoundException()
    {
        Question question = QuestionFakes.CreateQuestion();
        repositoryMock.Setup(r => r.GetByIdAsync(question.Id, default)).ReturnsAsync(question);
 
        UpdateAnswerWithIdsCommand command = new UpdateAnswerWithIdsCommand(question.Id, "nonexistent-answer", "text", Guid.NewGuid());
        var act = () => handler.Handle(command, default);
 
        await act.Should().ThrowAsync<NotFoundException>();
    }
}