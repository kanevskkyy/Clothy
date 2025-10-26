using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.ReviewService.Application.Interfaces.Commands;

namespace Clothy.ReviewService.Application.Features.Reviews.Commands.UpdateReview
{
    public record UpdateReviewWithIdCommand(
        string ReviewId,
        string Comment,
        int Rating
    ) : ICommand;
}