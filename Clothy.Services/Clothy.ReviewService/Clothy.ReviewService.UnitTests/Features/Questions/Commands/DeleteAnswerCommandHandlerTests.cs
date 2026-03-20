using Clothy.ReviewService.Application.Features.Questions.Commands.DeleteAnswer;
using Clothy.ReviewService.Domain.Entities;
using Clothy.ReviewService.Domain.Interfaces;
using Clothy.ReviewService.UnitTests.Helpers;
using Clothy.Shared.Helpers.Exceptions;
using FluentAssertions;
using Moq;
using Xunit;

namespace Clothy.ReviewService.UnitTests.Features.Questions.Commands;

public class DeleteAnswerCommandHandlerTests
{
    private Mock<IQuestionRepository> repositoryMock = new();
    private DeleteAnswerToQuestionCommandHandler handler;
 
    public DeleteAnswerCommandHandlerTests()
    {
        handler = new DeleteAnswerToQuestionCommandHandler(repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_QuestionOwnerDeletes_DeletesSuccessfully()
    {
        var (question, answer) = QuestionFakes.CreateQuestionWithAnswer();
        repositoryMock.Setup(r => r.GetByIdAsync(question.Id, default)).ReturnsAsync(question);
 
        DeleteAnswerToQuestionCommand command = new DeleteAnswerToQuestionCommand(question.Id, answer.Id, question.User.UserId, false, false);
        await handler.Handle(command, default);
 
        repositoryMock.Verify(r => r.DeleteAnswerAsync(question.Id, answer.Id, default), Times.Once);
    }

    [Fact]
    public async Task Handle_AdminDeletes_DeletesSuccessfully()
    {
        var (question, answer) = QuestionFakes.CreateQuestionWithAnswer();
        repositoryMock.Setup(r => r.GetByIdAsync(question.Id, default)).ReturnsAsync(question);
        
        DeleteAnswerToQuestionCommand command = new DeleteAnswerToQuestionCommand(question.Id, answer.Id, Guid.NewGuid(), IsAdmin: true, IsManager: false);
        await handler.Handle(command, default);
 
        repositoryMock.Verify(r => r.DeleteAnswerAsync(question.Id, answer.Id, default), Times.Once);
    }
    
    [Fact]
    public async Task Handle_ManagerDeletes_DeletesSuccessfully()
    {
        var (question, answer) = QuestionFakes.CreateQuestionWithAnswer();
        repositoryMock.Setup(r => r.GetByIdAsync(question.Id, default)).ReturnsAsync(question);
 
        DeleteAnswerToQuestionCommand command = new DeleteAnswerToQuestionCommand(question.Id, answer.Id, Guid.NewGuid(), IsAdmin: false, IsManager: true);
        await handler.Handle(command, default);
 
        repositoryMock.Verify(r => r.DeleteAnswerAsync(question.Id, answer.Id, default), Times.Once);
    }
 
    [Fact]
    public async Task Handle_UnauthorizedUserDeletes_ThrowsForbiddenException()
    {
        var (question, answer) = QuestionFakes.CreateQuestionWithAnswer();
        repositoryMock.Setup(r => r.GetByIdAsync(question.Id, default)).ReturnsAsync(question);
 
        DeleteAnswerToQuestionCommand command = new DeleteAnswerToQuestionCommand(question.Id, answer.Id, Guid.NewGuid(), false, false);
        var act = () => handler.Handle(command, default);
 
        await act.Should().ThrowAsync<ForbiddenException>();
        repositoryMock.Verify(r => r.DeleteAnswerAsync(It.IsAny<string>(), It.IsAny<string>(), default), Times.Never);
    }
 
    [Fact]
    public async Task Handle_QuestionNotFound_ThrowsNotFoundException()
    {
        repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<string>(), default)).ReturnsAsync((Question?)null);
 
        DeleteAnswerToQuestionCommand command = new DeleteAnswerToQuestionCommand("nonexistent-q", "some-answer", Guid.NewGuid(), false, false);
        var act = () => handler.Handle(command, default);
 
        await act.Should().ThrowAsync<NotFoundException>();
    }
 
    [Fact]
    public async Task Handle_AnswerNotFound_ThrowsNotFoundException()
    {
        Question question = QuestionFakes.CreateQuestion(); 
        repositoryMock.Setup(r => r.GetByIdAsync(question.Id, default)).ReturnsAsync(question);
 
        DeleteAnswerToQuestionCommand command = new DeleteAnswerToQuestionCommand(question.Id, "nonexistent-answer", question.User.UserId, false, false);
        var act = () => handler.Handle(command, default);
 
        await act.Should().ThrowAsync<NotFoundException>();
    }

}