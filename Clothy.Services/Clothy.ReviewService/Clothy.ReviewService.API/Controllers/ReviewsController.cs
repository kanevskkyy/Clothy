using Clothy.ReviewService.Application.Features.Reviews.Commands.CreateReview;
using Clothy.ReviewService.Application.Features.Reviews.Commands.DeleteReview;
using Clothy.ReviewService.Application.Features.Reviews.Commands.UpdateReview;
using Clothy.ReviewService.Application.Features.Reviews.Query.GetReviewById;
using Clothy.ReviewService.Application.Features.Reviews.Query.GetReviews;
using Clothy.ReviewService.Domain.Entities.QueryParameters;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Clothy.ReviewService.API.Controllers
{
    [ApiController]
    [Route("api/reviews")]
    public class ReviewsController : ControllerBase
    {
        private IMediator mediator;
        private ILogger<ReviewsController> logger;

        public ReviewsController(IMediator mediator, ILogger<ReviewsController> logger)
        {
            this.mediator = mediator;
            this.logger = logger;
        }

        /// <summary>
        /// Get paginated list of reviews with optional filtering and sorting.
        /// </summary>
        /// <param name="queryParams">Query parameters for reviews.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Paginated list of reviews.</returns>
        [HttpGet]
        public async Task<IActionResult> GetReviews([FromQuery] ReviewQueryParameters queryParams, CancellationToken cancellationToken)
        {
            logger.LogInformation("Fetching paged reviews. Page: {PageNumber}, PageSize: {PageSize}", queryParams.PageNumber, queryParams.PageSize);
            GetReviewsQuery query = new GetReviewsQuery(queryParams);
            
            var result = await mediator.Send(query, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Get a specific review by its ID.
        /// </summary>
        /// <param name="id">Review ID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Review details.</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetReviewById(string id, CancellationToken cancellationToken)
        {
            logger.LogInformation("Fetching review with ID: {Id}", id);
            GetReviewByIdQuery query = new GetReviewByIdQuery(id);
            
            var result = await mediator.Send(query, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Create a new review.
        /// </summary>
        /// <param name="command">Review creation data.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Created review object.</returns>
        [HttpPost]
        public async Task<IActionResult> CreateReview([FromBody] CreateReviewCommand command, CancellationToken cancellationToken)
        {
            logger.LogInformation("Creating review for ClotheItemId: {ClotheItemId}", command.ClotheItemId);

            var review = await mediator.Send(command, cancellationToken);

            logger.LogInformation("Review created with ID: {Id}", review.Id);

            return CreatedAtAction(nameof(GetReviewById), new { id = review.Id }, review);
        }


        /// <summary>
        /// Update an existing review by its ID.
        /// </summary>
        /// <param name="id">Review ID.</param>
        /// <param name="command">Update data for the review.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>No content.</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateReview(string id, [FromBody] UpdateReviewCommand command, CancellationToken cancellationToken)
        {
            logger.LogInformation("Updating review with ID: {Id}", id);

            await mediator.Send(new UpdateReviewWithIdCommand(id, command.Comment, command.Rating), cancellationToken);

            logger.LogInformation("Review with ID {Id} updated.", id);
            return NoContent();
        }

        /// <summary>
        /// Delete a review by its ID.
        /// </summary>
        /// <param name="id">Review ID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>No content.</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReview(string id, CancellationToken cancellationToken)
        {
            logger.LogInformation("Deleting review with ID: {Id}", id);
            DeleteReviewCommand command = new DeleteReviewCommand(id);
            
            await mediator.Send(command, cancellationToken);
            
            logger.LogInformation("Review with ID {Id} deleted.", id);
            return NoContent();
        }
    }
}
