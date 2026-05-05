using Clothy.ReviewService.Application.Features.Questions.Query.GetQuestionById;
using Clothy.ReviewService.Domain.Entities;
using Clothy.ReviewService.Domain.Interfaces;
using Clothy.ReviewService.UnitTests.Helpers;
using Clothy.Shared.Helpers.Exceptions;
using FluentAssertions;
using Moq;
using Xunit;

namespace Clothy.ReviewService.UnitTests.Features.Questions.Query;

public class GetQuestionByIdQueryHandlerTests
{
    private Mock<IQuestionRepository> repositoryMock = new();
    private GetQuestionByIdQueryHandler handler;
 
    public GetQuestionByIdQueryHandlerTests()
    {
        handler = new GetQuestionByIdQueryHandler(repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_QuestionExists_ReturnsQuestion()
    {
        Question question = QuestionFakes.CreateQuestion();
        repositoryMock.Setup(r => r.GetByIdAsync(question.Id, default)).ReturnsAsync(question);
        
        Question? result = await handler.Handle(new GetQuestionByIdQuery(question.Id), default);
        result.Should().Be(question);
    }

    [Fact]
    public async Task Handle_QuestionNotFound_ThrowsNotFoundException()
    {
        repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<string>(), default)).ReturnsAsync((Question?)null);
 
        var act = () => handler.Handle(new GetQuestionByIdQuery("nonexistent-id"), default);
 
        await act.Should().ThrowAsync<NotFoundException>();
    }
}