using Clothy.ReviewService.Application.Features.Questions.Commands.DeleteQuestion;
using Clothy.ReviewService.Domain.Entities;
using Clothy.ReviewService.Domain.Interfaces;
using Clothy.ReviewService.UnitTests.Helpers;
using Clothy.Shared.Helpers.Exceptions;
using FluentAssertions;
using Moq;
using Xunit;

namespace Clothy.ReviewService.UnitTests.Features.Questions.Commands;

public class DeleteQuestionCommandHandlerTests
{
    private Mock<IQuestionRepository> repositoryMock = new();
    private DeleteQuestionCommandHandler handler;
 
    public DeleteQuestionCommandHandlerTests()
    {
        handler = new DeleteQuestionCommandHandler(repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_OwnerDeletes_DeletesSuccessfully()
    {
        Question question = QuestionFakes.CreateQuestion();
        repositoryMock.Setup(r => r.GetByIdAsync(question.Id, default)).ReturnsAsync(question);    
        
        DeleteQuestionCommand command = new DeleteQuestionCommand(question.Id, question.User.UserId, false, false);
        await handler.Handle(command, default);
 
        repositoryMock.Verify(r => r.DeleteAsync(question.Id, default), Times.Once);
    }
    
    [Fact]
    public async Task Handle_AdminDeletes_DeletesSuccessfully()
    {
        Question question = QuestionFakes.CreateQuestion();
        repositoryMock.Setup(r => r.GetByIdAsync(question.Id, default)).ReturnsAsync(question);
 
        DeleteQuestionCommand command = new DeleteQuestionCommand(question.Id, Guid.NewGuid(), IsAdmin: true, IsManager: false);
        await handler.Handle(command, default);
 
        repositoryMock.Verify(r => r.DeleteAsync(question.Id, default), Times.Once);
    }
    
    [Fact]
    public async Task Handle_ManagerDeletes_DeletesSuccessfully()
    {
        Question question = QuestionFakes.CreateQuestion();
        repositoryMock.Setup(r => r.GetByIdAsync(question.Id, default)).ReturnsAsync(question);
 
        DeleteQuestionCommand command = new DeleteQuestionCommand(question.Id, Guid.NewGuid(), IsAdmin: false, IsManager: true);
        await handler.Handle(command, default);
 
        repositoryMock.Verify(r => r.DeleteAsync(question.Id, default), Times.Once);
    }
 
    [Fact]
    public async Task Handle_UnauthorizedUserDeletes_ThrowsForbiddenException()
    {
        Question question = QuestionFakes.CreateQuestion();
        repositoryMock.Setup(r => r.GetByIdAsync(question.Id, default)).ReturnsAsync(question);
 
        DeleteQuestionCommand command = new DeleteQuestionCommand(question.Id, Guid.NewGuid(), false, false);
        var act = () => handler.Handle(command, default);
 
        await act.Should().ThrowAsync<ForbiddenException>();
        repositoryMock.Verify(r => r.DeleteAsync(It.IsAny<string>(), default), Times.Never);
    }
 
    [Fact]
    public async Task Handle_QuestionNotFound_ThrowsNotFoundException()
    {
        repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<string>(), default)).ReturnsAsync((Question?)null);
 
        DeleteQuestionCommand command = new DeleteQuestionCommand("nonexistent-id", Guid.NewGuid(), false, false);
        var act = () => handler.Handle(command, default);
 
        await act.Should().ThrowAsync<NotFoundException>();
    }
}