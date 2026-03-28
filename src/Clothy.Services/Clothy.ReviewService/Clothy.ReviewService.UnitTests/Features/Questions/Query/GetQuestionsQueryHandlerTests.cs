using Clothy.ReviewService.Application.Features.Questions.Query.GetQuestions;
using Clothy.ReviewService.Domain.Entities;
using Clothy.ReviewService.Domain.Entities.QueryParameters;
using Clothy.ReviewService.Domain.Interfaces;
using Clothy.ReviewService.UnitTests.Helpers;
using Clothy.Shared.Helpers;
using FluentAssertions;
using Moq;
using Xunit;

namespace Clothy.ReviewService.UnitTests.Features.Questions.Query;

public class GetQuestionsQueryHandlerTests
{
    private Mock<IQuestionRepository> repositoryMock = new();
    private GetQuestionsQueryHandler handler;
 
    public GetQuestionsQueryHandlerTests()
    {
        handler = new GetQuestionsQueryHandler(repositoryMock.Object);
    }
 
    [Fact]
    public async Task Handle_ReturnsPagedListFromRepository()
    {
        QuestionQueryParameters queryParams = new QuestionQueryParameters();
        PagedList<Question> expected = new PagedList<Question>([QuestionFakes.CreateQuestion()], 1, 10, 1);
        repositoryMock.Setup(r => r.GetQuestionsAsync(queryParams, default)).ReturnsAsync(expected);
 
        PagedList<Question> result = await handler.Handle(new GetQuestionsQuery(queryParams), default);
 
        result.Should().BeEquivalentTo(expected);
    }
 
    [Fact]
    public async Task Handle_EmptyResult_ReturnsEmptyPagedList()
    {
        QuestionQueryParameters queryParams = new QuestionQueryParameters();
        PagedList<Question> expected = new PagedList<Question>([], 0, 1, 10);
        repositoryMock.Setup(r => r.GetQuestionsAsync(queryParams, default)).ReturnsAsync(expected);
 
        PagedList<Question> result = await handler.Handle(new GetQuestionsQuery(queryParams), default);
 
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }
 
    [Fact]
    public async Task Handle_PassesQueryParametersToRepository()
    {
        QuestionQueryParameters queryParams = new QuestionQueryParameters
        {
            PageNumber = 2
        };
        PagedList<Question> expected = new PagedList<Question>([], 2, 10, 0);
        repositoryMock.Setup(r => r.GetQuestionsAsync(queryParams, default)).ReturnsAsync(expected);
 
        await handler.Handle(new GetQuestionsQuery(queryParams), default);
 
        repositoryMock.Verify(r => r.GetQuestionsAsync(queryParams, default), Times.Once);
    }
}