using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.ReviewService.Application.Interfaces.Commands;
using Clothy.ReviewService.Domain.Entities;

namespace Clothy.ReviewService.Application.Features.Reviews.Commands.ConfirmReview
{
    public record ConfirmReviewCommand(string ReviewId) : ICommand<Review>;
}
