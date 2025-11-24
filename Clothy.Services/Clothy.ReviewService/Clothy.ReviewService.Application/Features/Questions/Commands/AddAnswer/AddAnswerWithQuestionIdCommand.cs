using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Clothy.ReviewService.Application.Interfaces.Commands;
using Clothy.ReviewService.Domain.Entities;
using Clothy.ReviewService.Domain.ValueObjects;

namespace Clothy.ReviewService.Application.Features.Questions.Commands.AddAnswer
{
    public record AddAnswerWithQuestionIdCommand(
        string QuestionId,
        Guid UserId,
        string FirstName,
        string LastName,
        string PhotoUrl,
        string AnswerText
    ) : ICommand<Answer>;
}
