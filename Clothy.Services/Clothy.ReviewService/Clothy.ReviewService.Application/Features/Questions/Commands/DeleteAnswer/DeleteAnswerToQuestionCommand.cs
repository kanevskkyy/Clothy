using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.ReviewService.Application.Interfaces.Commands;
using MediatR;

namespace Clothy.ReviewService.Application.Features.Questions.Commands.DeleteAnswer
{
    public record DeleteAnswerToQuestionCommand(
        string QuestionId,
        string AnswerId
    ) : ICommand;
}
