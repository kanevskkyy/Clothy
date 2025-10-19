using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.ReviewService.Application.Interfaces.Commands;
using Clothy.ReviewService.Domain.ValueObjects;
using MediatR;

namespace Clothy.ReviewService.Application.Features.Questions.Commands.AddAnswer
{
    public record AddAnswerToQuestionCommand(
        Guid QuestionId,
        UserInfo User,
        TextValue AnswerText
    ) : ICommand<Guid>;
}
