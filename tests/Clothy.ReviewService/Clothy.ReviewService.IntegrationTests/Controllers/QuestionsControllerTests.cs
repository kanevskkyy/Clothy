using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Clothy.ReviewService.Application.Features.Questions.Commands.AddAnswer;
using Clothy.ReviewService.Application.Features.Questions.Commands.CreateQuestion;
using Clothy.ReviewService.Application.Features.Questions.Commands.UpdateAnswer;
using Clothy.ReviewService.Application.Features.Questions.Commands.UpdateQuestion;
using Clothy.ReviewService.Domain.Entities;
using Clothy.ReviewService.Domain.Interfaces;
using Clothy.ReviewService.Domain.ValueObjects;
using Clothy.ReviewService.IntegrationTests.Infrastructure;
using Clothy.Shared.Helpers;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Clothy.ReviewService.IntegrationTests.Controllers;

[Collection("ReviewService")]
public class QuestionsControllerTests
{
    private HttpClient client;
    private ReviewServiceWebApplicationFactory factory;

    private static JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters =
        {
            new JsonStringEnumConverter()
        }
    };

    public QuestionsControllerTests(ReviewServiceWebApplicationFactory factory)
    {
        this.factory = factory;
        client = factory.CreateClient();
        factory.ClotheItemValidatorMock.Reset();
    }

    [Fact]
    public async Task GetQuestions_WithoutAuth_ShouldReturnOk()
    {
        Guid clotheId = Guid.NewGuid();
        await SeedQuestion(clotheId, Guid.NewGuid());

        HttpResponseMessage response = await client.GetAsync($"/api/questions?clotheItemId={clotheId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        PagedList<Question>? result = await response.Content.ReadFromJsonAsync<PagedList<Question>>(JsonOptions);
        result.Should().NotBeNull();
        result!.Items.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetQuestions_DefaultPagination_ShouldReturnPagedResult()
    {
        Guid clotheId = Guid.NewGuid();
        await SeedQuestion(clotheId, Guid.NewGuid());
        await SeedQuestion(clotheId, Guid.NewGuid());

        HttpResponseMessage response = await client.GetAsync($"/api/questions?clotheItemId={clotheId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        PagedList<Question>? result = await response.Content.ReadFromJsonAsync<PagedList<Question>>(JsonOptions);
        result!.Items.Count.Should().BeGreaterThanOrEqualTo(2);
    }


    [Fact]
    public async Task CreateQuestion_WithoutAuth_ShouldReturnUnauthorized()
    {
        CreateQuestionDTO dto = new()
        {
            ClotheItemId = Guid.NewGuid(),
            QuestionText = "Does this come in blue?"
        };

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/questions", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateQuestion_WithValidData_ShouldReturnCreated()
    {
        Guid userId = Guid.NewGuid();
        Guid clotheId = Guid.NewGuid();
        client.AddAuthorizationHeader(userId);

        factory.ClotheItemValidatorMock.SetupValidClotheItem(clotheId);

        CreateQuestionDTO dto = new()
        {
            ClotheItemId = clotheId,
            QuestionText = "What material is this made of?"
        };

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/questions", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        Question? result = await response.Content.ReadFromJsonAsync<Question>(JsonOptions);
        result.Should().NotBeNull();
        result!.ClotheInfo.ClotheItemId.Should().Be(clotheId);
        result.User.UserId.Should().Be(userId);
        result.QuestionText.Should().Be(dto.QuestionText);
    }

    [Fact]
    public async Task CreateQuestion_WhenClotheItemInvalid_ShouldReturnBadRequest()
    {
        Guid userId = Guid.NewGuid();
        Guid clotheId = Guid.NewGuid();
        client.AddAuthorizationHeader(userId);

        factory.ClotheItemValidatorMock.SetupInvalidClotheItem(clotheId);

        CreateQuestionDTO dto = new()
        {
            ClotheItemId = clotheId,
            QuestionText = "Is this available in XL?"
        };

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/questions", dto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateQuestion_WithoutAuth_ShouldReturnUnauthorized()
    {
        HttpResponseMessage response = await client.PutAsJsonAsync(
            $"/api/questions/{Guid.NewGuid()}",
            new UpdateQuestionDTO
            {
                QuestionText = "Updated text"
            });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateQuestion_ByOwner_ShouldReturnNoContent()
    {
        Guid userId = Guid.NewGuid();
        Question seeded = await SeedQuestion(Guid.NewGuid(), userId);
        client.AddAuthorizationHeader(userId);

        HttpResponseMessage response = await client.PutAsJsonAsync(
            $"/api/questions/{seeded.Id}",
            new UpdateQuestionDTO
            {
                QuestionText = "Updated question text"
            });

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task UpdateQuestion_ByAnotherUser_ShouldReturnForbidden()
    {
        Guid owner = Guid.NewGuid();
        Guid attacker = Guid.NewGuid();
        Question seeded = await SeedQuestion(Guid.NewGuid(), owner);
        client.AddAuthorizationHeader(attacker);

        HttpResponseMessage response = await client.PutAsJsonAsync(
            $"/api/questions/{seeded.Id}",
            new UpdateQuestionDTO
            {
                QuestionText = "Hacked question"
            });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeleteQuestion_WithoutAuth_ShouldReturnUnauthorized()
    {
        HttpResponseMessage response = await client.DeleteAsync($"/api/questions/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeleteQuestion_ByOwner_ShouldReturnNoContent()
    {
        Guid userId = Guid.NewGuid();
        Question seeded = await SeedQuestion(Guid.NewGuid(), userId);
        client.AddAuthorizationHeader(userId);

        HttpResponseMessage response = await client.DeleteAsync($"/api/questions/{seeded.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteQuestion_ByAnotherUser_ShouldReturnForbidden()
    {
        Question seeded = await SeedQuestion(Guid.NewGuid(), Guid.NewGuid());
        client.AddAuthorizationHeader(Guid.NewGuid());

        HttpResponseMessage response = await client.DeleteAsync($"/api/questions/{seeded.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeleteQuestion_ByAdmin_ShouldReturnNoContent()
    {
        Question seeded = await SeedQuestion(Guid.NewGuid(), Guid.NewGuid());
        client.AddAuthorizationHeader(Guid.NewGuid(), roles: new[] { "Admin" });

        HttpResponseMessage response = await client.DeleteAsync($"/api/questions/{seeded.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteQuestion_ByManager_ShouldReturnNoContent()
    {
        Question seeded = await SeedQuestion(Guid.NewGuid(), Guid.NewGuid());
        client.AddAuthorizationHeader(Guid.NewGuid(), roles: new[] { "Manager" });

        HttpResponseMessage response = await client.DeleteAsync($"/api/questions/{seeded.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task AddAnswer_WithoutAuth_ShouldReturnUnauthorized()
    {
        HttpResponseMessage response = await client.PostAsJsonAsync(
            $"/api/questions/{Guid.NewGuid()}/answers",
            new AddAnswerDTO
            {
                AnswerText = "Some answer"
            });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AddAnswer_ByAnotherUser_ShouldReturnOk()
    {
        Guid owner = Guid.NewGuid();
        Guid respondent = Guid.NewGuid();
        Question seeded = await SeedQuestion(Guid.NewGuid(), owner);
        client.AddAuthorizationHeader(respondent);

        HttpResponseMessage response = await client.PostAsJsonAsync(
            $"/api/questions/{seeded.Id}/answers",
            new AddAnswerDTO
            {
                AnswerText = "Great question! It fits true to size."
            });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        Answer? result = await response.Content.ReadFromJsonAsync<Answer>(JsonOptions);
        result.Should().NotBeNull();
        result!.AnswerText.Should().Be("Great question! It fits true to size.");
        result.User.UserId.Should().Be(respondent);
    }

    [Fact]
    public async Task AddAnswer_ByOwner_ShouldReturnBadRequest()
    {
        Guid owner = Guid.NewGuid();
        Question seeded = await SeedQuestion(Guid.NewGuid(), owner);
        client.AddAuthorizationHeader(owner);

        HttpResponseMessage response = await client.PostAsJsonAsync(
            $"/api/questions/{seeded.Id}/answers",
            new AddAnswerDTO
            {
                AnswerText = "Answering my own question"
            });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateAnswer_WithoutAuth_ShouldReturnUnauthorized()
    {
        HttpResponseMessage response = await client.PutAsJsonAsync(
            $"/api/questions/{Guid.NewGuid()}/answers/{Guid.NewGuid()}",
            new UpdateAnswerDTO
            {
                AnswerText = "Updated"
            });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateAnswer_ByAnotherUser_ShouldReturnForbidden()
    {
        Guid questionOwner = Guid.NewGuid();
        Guid answerer = Guid.NewGuid();
        Guid attacker = Guid.NewGuid();
        Question seeded = await SeedQuestion(Guid.NewGuid(), questionOwner);
        Answer answer = await SeedAnswer(seeded.Id, answerer);
        client.AddAuthorizationHeader(attacker);

        HttpResponseMessage response = await client.PutAsJsonAsync(
            $"/api/questions/{seeded.Id}/answers/{answer.Id}",
            new UpdateAnswerDTO
            {
                AnswerText = "Hacked"
            });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateAnswer_AnswerNotFound_ShouldReturn404()
    {
        Guid userId = Guid.NewGuid();
        Question seeded = await SeedQuestion(Guid.NewGuid(), Guid.NewGuid());
        client.AddAuthorizationHeader(userId);

        HttpResponseMessage response = await client.PutAsJsonAsync(
            $"/api/questions/{seeded.Id}/answers/{Guid.NewGuid()}",
            new UpdateAnswerDTO
            {
                AnswerText = "Test"
            });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteAnswer_WithoutAuth_ShouldReturnUnauthorized()
    {
        HttpResponseMessage response = await client.DeleteAsync(
            $"/api/questions/{Guid.NewGuid()}/answers/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeleteAnswer_ByAnswerOwner_ShouldReturnNoContent()
    {
        Guid questionOwner = Guid.NewGuid();
        Guid answerer = Guid.NewGuid();
        Question seeded = await SeedQuestion(Guid.NewGuid(), questionOwner);
        Answer answer = await SeedAnswer(seeded.Id, answerer);
        client.AddAuthorizationHeader(answerer);

        HttpResponseMessage response = await client.DeleteAsync(
            $"/api/questions/{seeded.Id}/answers/{answer.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeleteAnswer_ByAnotherUser_ShouldReturnForbidden()
    {
        Guid questionOwner = Guid.NewGuid();
        Guid answerer = Guid.NewGuid();
        Question seeded = await SeedQuestion(Guid.NewGuid(), questionOwner);
        Answer answer = await SeedAnswer(seeded.Id, answerer);
        client.AddAuthorizationHeader(Guid.NewGuid());

        HttpResponseMessage response = await client.DeleteAsync(
            $"/api/questions/{seeded.Id}/answers/{answer.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeleteAnswer_ByAdmin_ShouldReturnNoContent()
    {
        Guid questionOwner = Guid.NewGuid();
        Guid answerer = Guid.NewGuid();
        Question seeded = await SeedQuestion(Guid.NewGuid(), questionOwner);
        Answer answer = await SeedAnswer(seeded.Id, answerer);
        client.AddAuthorizationHeader(Guid.NewGuid(), roles: new[] { "Admin" });

        HttpResponseMessage response = await client.DeleteAsync(
            $"/api/questions/{seeded.Id}/answers/{answer.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteAnswer_ByManager_ShouldReturnNoContent()
    {
        Guid questionOwner = Guid.NewGuid();
        Guid answerer = Guid.NewGuid();
        Question seeded = await SeedQuestion(Guid.NewGuid(), questionOwner);
        Answer answer = await SeedAnswer(seeded.Id, answerer);
        client.AddAuthorizationHeader(Guid.NewGuid(), roles: new[] { "Manager" });

        HttpResponseMessage response = await client.DeleteAsync(
            $"/api/questions/{seeded.Id}/answers/{answer.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteAnswer_AnswerNotFound_ShouldReturn404()
    {
        Question seeded = await SeedQuestion(Guid.NewGuid(), Guid.NewGuid());
        client.AddAuthorizationHeader(Guid.NewGuid(), roles: new[] { "Admin" });

        HttpResponseMessage response = await client.DeleteAsync(
            $"/api/questions/{seeded.Id}/answers/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private async Task<Question> SeedQuestion(Guid clotheId, Guid userId)
    {
        using var scope = factory.Services.CreateScope();
        IQuestionRepository repo = scope.ServiceProvider.GetRequiredService<IQuestionRepository>();

        Question question = new Question(
            new ClotheInfo(clotheId, "Test Clothe", "https://photo.test/img.jpg"),
            new UserInfo(userId, "Test", "User", "https://photo.test/user.jpg"),
            questionText: "Seeded question?"
        );

        await repo.AddAsync(question, CancellationToken.None);
        return question;
    }

    private async Task<Answer> SeedAnswer(string questionId, Guid userId)
    {
        using var scope = factory.Services.CreateScope();
        IQuestionRepository repo = scope.ServiceProvider.GetRequiredService<IQuestionRepository>();

        Answer answer = new Answer(
            new UserInfo(userId, "Answer", "User", "https://photo.test/user.jpg"),
            answerText: "Seeded answer"
        );

        await repo.AddAnswerAsync(questionId, answer, CancellationToken.None);
        return answer;
    }
}