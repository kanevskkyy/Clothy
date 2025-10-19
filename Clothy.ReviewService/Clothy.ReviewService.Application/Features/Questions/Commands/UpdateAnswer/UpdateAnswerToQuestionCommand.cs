using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.ReviewService.Application.Interfaces.Commands;
using Clothy.ReviewService.Domain.ValueObjects;
using MediatR;

namespace Clothy.ReviewService.Application.Features.Questions.Commands.UpdateAnswer
{
    public record UpdateAnswerToQuestionCommand(
        Guid QuestionId,
        Guid AnswerId,
        TextValue AnswerText
    ) : ICommand;
}
