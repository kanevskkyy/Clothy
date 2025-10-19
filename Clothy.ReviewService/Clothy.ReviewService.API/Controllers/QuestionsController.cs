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
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Clothy.ReviewService.API.Controllers
{
    [ApiController]
    [Route("api/questions")]
    public class QuestionsController : ControllerBase
    {
        private IMediator mediator;
        private ILogger<QuestionsController> logger;

        public QuestionsController(IMediator mediator, ILogger<QuestionsController> logger)
        {
            this.mediator = mediator;
            this.logger = logger;
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
            
            var result = await mediator.Send(query, cancellationToken);
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
            
            var result = await mediator.Send(query, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Create a new question.
        /// </summary>
        /// <param name="command">Question creation data.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Created question ID.</returns>
        [HttpPost]
        public async Task<IActionResult> CreateQuestion([FromBody] CreateQuestionCommand command, CancellationToken cancellationToken)
        {
            logger.LogInformation("Creating question for ClotheItemId: {ClotheItemId}", command.ClotheItemId);

            var question = await mediator.Send(command, cancellationToken);

            logger.LogInformation("Question created with ID: {Id}", question.Id);
            return CreatedAtAction(nameof(GetQuestionById), new { 
                id = question.Id 
            }, question);
        }


        /// <summary>
        /// Update an existing question by its ID.
        /// </summary>
        /// <param name="id">Question ID.</param>
        /// <param name="command">Update data for the question.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>No content.</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateQuestion(string id, [FromBody] UpdateQuestionCommand command, CancellationToken cancellationToken)
        {
            logger.LogInformation("Updating question with ID: {Id}", id);

            await mediator.Send(new UpdateQuestionWithIdCommand(id, command.QuestionText), cancellationToken);

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
        public async Task<IActionResult> DeleteQuestion(string id, CancellationToken cancellationToken)
        {
            logger.LogInformation("Deleting question with ID: {Id}", id);
            DeleteQuestionCommand command = new DeleteQuestionCommand(id);
            
            await mediator.Send(command, cancellationToken);
            
            logger.LogInformation("Question with ID {Id} deleted.", id);
            return NoContent();
        }

        /// <summary>
        /// Add an answer to a question.
        /// </summary>
        /// <param name="id">Question ID.</param>
        /// <param name="command">Answer data.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Created answer object.</returns>
        [HttpPost("{id}/answers")]
        public async Task<IActionResult> AddAnswer(string id, [FromBody] AddAnswerCommand command, CancellationToken cancellationToken)
        {
            logger.LogInformation("Adding answer to question ID: {Id}", id);

            Answer answer = await mediator.Send(new AddAnswerWithQuestionIdCommand(id, command.User, command.AnswerText), cancellationToken);

            logger.LogInformation("Answer added to question ID {Id}", id);

            return Ok(answer);
        }



        /// <summary>
        /// Update an answer of a question.
        /// </summary>
        /// <param name="questionId">Question ID.</param>
        /// <param name="answerId">Answer ID.</param>
        /// <param name="command">Updated answer data.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>No content.</returns>
        [HttpPut("{questionId}/answers/{answerId}")]
        public async Task<IActionResult> UpdateAnswer(string questionId, string answerId, [FromBody] UpdateAnswerCommand command, CancellationToken cancellationToken)
        {
            logger.LogInformation("Updating answer {AnswerId} for question {QuestionId}", answerId, questionId);

            await mediator.Send(new UpdateAnswerWithIdsCommand(questionId, answerId, command.AnswerText), cancellationToken);

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
        public async Task<IActionResult> DeleteAnswer(string questionId, string answerId, CancellationToken cancellationToken)
        {
            logger.LogInformation("Deleting answer {AnswerId} from question {QuestionId}", answerId, questionId);
            DeleteAnswerToQuestionCommand command = new DeleteAnswerToQuestionCommand(questionId, answerId);
            
            await mediator.Send(command, cancellationToken);
            
            logger.LogInformation("Answer {AnswerId} deleted from question {QuestionId}", answerId, questionId);
            return NoContent();
        }
    }
}
