using System.Security.Claims;
using Clothy.ReviewService.API.Controllers;
using Clothy.ReviewService.Application.Features.Questions.Commands.AddAnswer;
using Clothy.ReviewService.Application.Features.Questions.Commands.CreateQuestion;
using Clothy.ReviewService.Application.Features.Questions.Commands.DeleteAnswer;
using Clothy.ReviewService.Application.Features.Questions.Commands.DeleteQuestion;
using Clothy.ReviewService.Application.Features.Questions.Commands.UpdateAnswer;
using Clothy.ReviewService.Application.Features.Questions.Commands.UpdateQuestion;
using Clothy.ReviewService.Application.Features.Questions.Query.GetQuestionById;
using Clothy.ReviewService.Application.Features.Questions.Query.GetQuestions;
using Clothy.ReviewService.Domain.Entities;
using Clothy.ReviewService.Domain.Entities.QueryParameters;
using Clothy.ReviewService.Domain.ValueObjects;
using Clothy.Shared.Helpers;
using Clothy.Shared.Helpers.JWT;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Clothy.ReviewService.UnitTests.Controllers;

public class QuestionsControllerTests
{
    private Mock<IMediator> mediatorMock;
    private Mock<ILogger<QuestionsController>> loggerMock;
    private Mock<IUserClaimsExtractor> claimsExtractorMock;
    private QuestionsController sut;

    private Guid USER_ID = Guid.NewGuid();
    private const string FIRST_NAME = "John";
    private const string LAST_NAME = "Doe";
    private const string PHOTO_URL = "https://example.com/photo.jpg";

    public QuestionsControllerTests()
    {
        mediatorMock = new Mock<IMediator>();
        loggerMock = new Mock<ILogger<QuestionsController>>();
        claimsExtractorMock = new Mock<IUserClaimsExtractor>();

        sut = new QuestionsController(
            mediatorMock.Object,
            loggerMock.Object,
            claimsExtractorMock.Object);

        sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity())
            }
        };

        claimsExtractorMock.Setup(x => x.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(USER_ID);
        claimsExtractorMock.Setup(x => x.GetFirstName(It.IsAny<ClaimsPrincipal>())).Returns(FIRST_NAME);
        claimsExtractorMock.Setup(x => x.GetLastName(It.IsAny<ClaimsPrincipal>())).Returns(LAST_NAME);
        claimsExtractorMock.Setup(x => x.GetPhotoUrl(It.IsAny<ClaimsPrincipal>())).Returns(PHOTO_URL);
        claimsExtractorMock.Setup(x => x.IsInRole(It.IsAny<ClaimsPrincipal>(), "Admin")).Returns(false);
        claimsExtractorMock.Setup(x => x.IsInRole(It.IsAny<ClaimsPrincipal>(), "Manager")).Returns(false);
    }
    
    [Fact]
    public async Task GetQuestions_ReturnsOk_WithPagedResult()
    {
        QuestionQueryParameters queryParams = new QuestionQueryParameters
        {
            PageNumber = 1,
            PageSize = 10 
        };
        PagedList<Question> pagedList = new PagedList<Question>(new List<Question>(), 0, 1, 10);

        mediatorMock
            .Setup(m => m.Send(It.IsAny<GetQuestionsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedList);

        IActionResult result = await sut.GetQuestions(queryParams, CancellationToken.None);

        OkObjectResult ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(pagedList);

        mediatorMock.Verify(m => m.Send(
            It.Is<GetQuestionsQuery>(q => q.QueryParameters == queryParams),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetQuestionById_ReturnsOk_WhenQuestionExists()
    {
        Question question = BuildQuestion();

        mediatorMock
            .Setup(m => m.Send(It.IsAny<GetQuestionByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(question);

        IActionResult result = await sut.GetQuestionById(question.Id, CancellationToken.None);
        
        OkObjectResult ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(question);

        mediatorMock.Verify(m => m.Send(
            It.Is<GetQuestionByIdQuery>(q => q.QuestionId == question.Id),
            It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact]
    public async Task CreateQuestion_ReturnsCreatedAtAction_WithQuestion()
    {
        CreateQuestionDTO dto = new CreateQuestionDTO
        {
            ClotheItemId = Guid.NewGuid(),
            QuestionText = "Does this shirt run small?"
        };
        Question question = BuildQuestion();

        mediatorMock
            .Setup(m => m.Send(It.IsAny<CreateQuestionCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(question);

        IActionResult result = await sut.CreateQuestion(dto, CancellationToken.None);

        CreatedAtActionResult created = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.ActionName.Should().Be(nameof(sut.GetQuestionById));
        created.RouteValues!["id"].Should().Be(question.Id);
        created.Value.Should().Be(question);

        mediatorMock.Verify(m => m.Send(
            It.Is<CreateQuestionCommand>(c =>
                c.ClotheItemId == dto.ClotheItemId &&
                c.UserId == USER_ID &&
                c.FirstName == FIRST_NAME &&
                c.LastName == LAST_NAME &&
                c.PhotoUrl == PHOTO_URL &&
                c.QuestionText == dto.QuestionText),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateQuestion_ReturnsNoContent_OnSuccess()
    {
        const string questionId = "q-001";
        UpdateQuestionDTO dto = new UpdateQuestionDTO
        {
            QuestionText = "Updated text"
        };

        mediatorMock
            .Setup(m => m.Send(It.IsAny<UpdateQuestionWithIdCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Unit.Value);

        IActionResult result = await sut.UpdateQuestion(questionId, dto, CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();

        mediatorMock.Verify(m => m.Send(
            It.Is<UpdateQuestionWithIdCommand>(c =>
                c.QuestionId == questionId &&
                c.QuestionText == dto.QuestionText &&
                c.UserId == USER_ID),
            It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact]
    public async Task DeleteQuestion_AsRegularUser_ReturnsNoContent()
    {
        const string questionId = "q-001";

        mediatorMock
            .Setup(m => m.Send(It.IsAny<DeleteQuestionCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Unit.Value);
        
        IActionResult result = await sut.DeleteQuestion(questionId, CancellationToken.None);
        
        result.Should().BeOfType<NoContentResult>();

        mediatorMock.Verify(m => m.Send(
            It.Is<DeleteQuestionCommand>(c =>
                c.QuestionId == questionId &&
                c.UserId == USER_ID &&
                c.IsAdmin == false &&
                c.IsManager == false),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteQuestion_AsAdmin_PassesIsAdminTrue()
    {
        const string questionId = "q-002";

        claimsExtractorMock.Setup(x => x.IsInRole(It.IsAny<ClaimsPrincipal>(), "Admin")).Returns(true);

        mediatorMock
            .Setup(m => m.Send(It.IsAny<DeleteQuestionCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Unit.Value);
        
        IActionResult result = await sut.DeleteQuestion(questionId, CancellationToken.None);
        result.Should().BeOfType<NoContentResult>();

        mediatorMock.Verify(m => m.Send(
            It.Is<DeleteQuestionCommand>(c => c.IsAdmin == true),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteQuestion_AsManager_PassesIsManagerTrue()
    {
        const string questionId = "q-003";

        claimsExtractorMock.Setup(x => x.IsInRole(It.IsAny<ClaimsPrincipal>(), "Manager")).Returns(true);

        mediatorMock
            .Setup(m => m.Send(It.IsAny<DeleteQuestionCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Unit.Value);

        IActionResult result = await sut.DeleteQuestion(questionId, CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();

        mediatorMock.Verify(m => m.Send(
            It.Is<DeleteQuestionCommand>(c => c.IsManager == true),
            It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact]
    public async Task AddAnswer_ReturnsOk_WithCreatedAnswer()
    {
        const string questionId = "q-001";
        AddAnswerDTO dto = new AddAnswerDTO
        {
            AnswerText = "Yes, it runs small." 
        };
        Answer answer = BuildAnswer();

        mediatorMock
            .Setup(m => m.Send(It.IsAny<AddAnswerWithQuestionIdCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(answer);

        IActionResult result = await sut.AddAnswer(questionId, dto, CancellationToken.None);
        
        OkObjectResult ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(answer);

        mediatorMock.Verify(m => m.Send(
            It.Is<AddAnswerWithQuestionIdCommand>(c =>
                c.QuestionId == questionId &&
                c.UserId == USER_ID &&
                c.FirstName == FIRST_NAME &&
                c.LastName == LAST_NAME &&
                c.PhotoUrl == PHOTO_URL &&
                c.AnswerText == dto.AnswerText),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAnswer_ReturnsNoContent_OnSuccess()
    {
        const string questionId = "q-001";
        const string answerId = "a-001";
        UpdateAnswerDTO dto = new UpdateAnswerDTO
        {
            AnswerText = "Updated answer"
        };

        mediatorMock
            .Setup(m => m.Send(It.IsAny<UpdateAnswerWithIdsCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Unit.Value);

        IActionResult result = await sut.UpdateAnswer(questionId, answerId, dto, CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();

        mediatorMock.Verify(m => m.Send(
            It.Is<UpdateAnswerWithIdsCommand>(c =>
                c.QuestionId == questionId &&
                c.AnswerId == answerId &&
                c.AnswerText == dto.AnswerText &&
                c.UserId == USER_ID),
            It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact]
    public async Task DeleteAnswer_AsRegularUser_ReturnsNoContent()
    {
        const string questionId = "q-001";
        const string answerId = "a-001";

        mediatorMock
            .Setup(m => m.Send(It.IsAny<DeleteAnswerToQuestionCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Unit.Value);

        IActionResult result = await sut.DeleteAnswer(questionId, answerId, CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();

        mediatorMock.Verify(m => m.Send(
            It.Is<DeleteAnswerToQuestionCommand>(c =>
                c.QuestionId == questionId &&
                c.AnswerId == answerId &&
                c.UserId == USER_ID &&
                c.IsAdmin == false &&
                c.IsManager == false),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAnswer_AsAdmin_PassesIsAdminTrue()
    {
        claimsExtractorMock.Setup(x => x.IsInRole(It.IsAny<ClaimsPrincipal>(), "Admin")).Returns(true);

        mediatorMock
            .Setup(m => m.Send(It.IsAny<DeleteAnswerToQuestionCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Unit.Value);

        IActionResult result = await sut.DeleteAnswer("q-001", "a-001", CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();

        mediatorMock.Verify(m => m.Send(
            It.Is<DeleteAnswerToQuestionCommand>(c => c.IsAdmin == true),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    private Question BuildQuestion()
    {
        ClotheInfo clotheInfo = new ClotheInfo(Guid.NewGuid(), "Cool T-Shirt", "https://example.com/shirt.jpg");
        UserInfo userInfo = new UserInfo(USER_ID, FIRST_NAME, LAST_NAME, PHOTO_URL);
        
        return new Question(clotheInfo, userInfo, "Does this shirt run small?");
    }

    private Answer BuildAnswer()
    {
        UserInfo userInfo = new UserInfo(Guid.NewGuid(), "Jane", "Smith", "https://example.com/jane.jpg");
        
        return new Answer(userInfo, "Yes, it runs small.");
    }
}