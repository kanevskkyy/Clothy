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
using Clothy.Shared.Helpers;
using Clothy.Shared.Helpers.JWT;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clothy.ReviewService.API.Controllers
{
    [ApiController]
    [Route("api/questions")]
    public class QuestionsController : ControllerBase
    {
        private IMediator mediator;
        private ILogger<QuestionsController> logger;
        private IUserClaimsExtractor userClaimsExtractor;

        public QuestionsController(IMediator mediator, ILogger<QuestionsController> logger, IUserClaimsExtractor userClaimsExtractor)
        {
            this.mediator = mediator;
            this.logger = logger;
            this.userClaimsExtractor = userClaimsExtractor;
        }

        /// <summary>
        /// Get paginated list of questions with optional filtering and sorting.
        /// </summary>
        /// <param name="queryParams">Query parameters for questions.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Paginated list of questions.</returns>
        [HttpGet]
        public async Task<IActionResult> GetQuestions([FromQuery] QuestionQueryParameters queryParams, CancellationToken cancellationToken)
        {
            logger.LogInformation("Fetching paged questions. Page: {PageNumber}, PageSize: {PageSize}", queryParams.PageNumber, queryParams.PageSize);
            GetQuestionsQuery query = new GetQuestionsQuery(queryParams);
            
            PagedList<Question> result = await mediator.Send(query, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Get a specific question by its ID.
        /// </summary>
        /// <param name="id">Question ID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Question details.</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetQuestionById(string id, CancellationToken cancellationToken)
        {
            logger.LogInformation("Fetching question with ID: {Id}", id);
            GetQuestionByIdQuery query = new GetQuestionByIdQuery(id);
            
            Question? result = await mediator.Send(query, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Create a new question.
        /// </summary>
        /// <param name="questionDTO">Question creation data.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Created question ID.</returns>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateQuestion([FromBody] CreateQuestionDTO questionDTO, CancellationToken cancellationToken)
        {
            logger.LogInformation("Creating question for ClotheItemId: {ClotheItemId}", questionDTO.ClotheItemId);

            Guid userId = userClaimsExtractor.GetUserId(User);
            string firstName = userClaimsExtractor.GetFirstName(User);
            string lastName = userClaimsExtractor.GetLastName(User);
            string photoUrl = userClaimsExtractor.GetPhotoUrl(User);

            CreateQuestionCommand command = new CreateQuestionCommand(questionDTO.ClotheItemId, userId, firstName, lastName, photoUrl, questionDTO.QuestionText);

            Question question = await mediator.Send(command, cancellationToken);

            logger.LogInformation("Question created with ID: {Id}", question.Id);
            return CreatedAtAction(nameof(GetQuestionById), new { id = question.Id }, question);
        }


        /// <summary>
        /// Update an existing question by its ID.
        /// </summary>
        /// <param name="id">Question ID.</param>
        /// <param name="dto">Update data for the question.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>No content.</returns>
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateQuestion(string id, [FromBody] UpdateQuestionDTO dto, CancellationToken cancellationToken)
        {
            logger.LogInformation("Updating question with ID: {Id}", id);

            Guid userId = userClaimsExtractor.GetUserId(User);
            UpdateQuestionWithIdCommand command = new UpdateQuestionWithIdCommand(id, dto.QuestionText, userId);

            await mediator.Send(command, cancellationToken);

            logger.LogInformation("Question with ID {Id} updated.", id);
            return NoContent();
        }


        /// <summary>
        /// Delete a question by its ID.
        /// </summary>
        /// <param name="id">Question ID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>No content.</returns>
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteQuestion(string id, CancellationToken cancellationToken)
        {
            logger.LogInformation("Deleting question with ID: {Id}", id);
            Guid userId = userClaimsExtractor.GetUserId(User);
            bool isAdmin = userClaimsExtractor.IsInRole(User, "Admin");
            bool isManager = userClaimsExtractor.IsInRole(User, "Manager");

            DeleteQuestionCommand command = new DeleteQuestionCommand(id, userId, isAdmin, isManager);

            await mediator.Send(command, cancellationToken);
            
            logger.LogInformation("Question with ID {Id} deleted.", id);
            return NoContent();
        }

        /// <summary>
        /// Add an answer to a question.
        /// </summary>
        /// <param name="id">Question ID.</param>
        /// <param name="dto">Answer data.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Created answer object.</returns>
        [HttpPost("{id}/answers")]
        [Authorize]
        public async Task<IActionResult> AddAnswer(string id, [FromBody] AddAnswerDTO dto, CancellationToken cancellationToken)
        {
            logger.LogInformation("Adding answer to question ID: {Id}", id);

            Guid userId = userClaimsExtractor.GetUserId(User);
            string firstName = userClaimsExtractor.GetFirstName(User);
            string lastName = userClaimsExtractor.GetLastName(User);
            string photoUrl = userClaimsExtractor.GetPhotoUrl(User);

            AddAnswerWithQuestionIdCommand command = new AddAnswerWithQuestionIdCommand(id, userId, firstName, lastName, photoUrl, dto.AnswerText);
            Answer answer = await mediator.Send(command, cancellationToken);

            logger.LogInformation("Answer added to question ID {Id}", id);

            return Ok(answer);
        }



        /// <summary>
        /// Update an answer of a question.
        /// </summary>
        /// <param name="questionId">Question ID.</param>
        /// <param name="answerId">Answer ID.</param>
        /// <param name="dto">Updated answer data.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>No content.</returns>
        [HttpPut("{questionId}/answers/{answerId}")]
        [Authorize]
        public async Task<IActionResult> UpdateAnswer(string questionId, string answerId, [FromBody] UpdateAnswerDTO dto, CancellationToken cancellationToken)
        {
            logger.LogInformation("Updating answer {AnswerId} for question {QuestionId}", answerId, questionId);

            Guid userId = userClaimsExtractor.GetUserId(User);
            UpdateAnswerWithIdsCommand command = new UpdateAnswerWithIdsCommand(questionId, answerId, dto.AnswerText, userId);

            await mediator.Send(command, cancellationToken);

            logger.LogInformation("Answer {AnswerId} updated for question {QuestionId}", answerId, questionId);
            return NoContent();
        }


        /// <summary>
        /// Delete an answer from a question.
        /// </summary>
        /// <param name="questionId">Question ID.</param>
        /// <param name="answerId">Answer ID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>No content.</returns>
        [HttpDelete("{questionId}/answers/{answerId}")]
        [Authorize]
        public async Task<IActionResult> DeleteAnswer(string questionId, string answerId, CancellationToken cancellationToken)
        {
            logger.LogInformation("Deleting answer {AnswerId} from question {QuestionId}", answerId, questionId);
            Guid userId = userClaimsExtractor.GetUserId(User);
            bool isAdmin = userClaimsExtractor.IsInRole(User, "Admin");
            bool isManager = userClaimsExtractor.IsInRole(User, "Manager");

            DeleteAnswerToQuestionCommand command = new DeleteAnswerToQuestionCommand(questionId, answerId, userId, isAdmin, isManager);

            await mediator.Send(command, cancellationToken);
            
            logger.LogInformation("Answer {AnswerId} deleted from question {QuestionId}", answerId, questionId);
            return NoContent();
        }
    }
}
