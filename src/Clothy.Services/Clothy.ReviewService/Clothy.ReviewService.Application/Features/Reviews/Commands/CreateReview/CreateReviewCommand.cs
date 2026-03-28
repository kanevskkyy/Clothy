using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Clothy.ReviewService.Application.Interfaces.Commands;
using Clothy.ReviewService.Domain.Entities;
using Clothy.ReviewService.Domain.ValueObjects;
using MediatR;

namespace Clothy.ReviewService.Application.Features.Reviews.Commands.CreateReview
{
    public record CreateReviewCommand(
        Guid ClotheItemId,
        int Rating,
        string Comment,
        Guid UserId,
        string FirstName,
        string LastName,
        string PhotoUrl
    ) : ICommand<Review>;
}
