using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.ReviewService.Application.Interfaces.Commands;

namespace Clothy.ReviewService.Application.Features.Questions.Commands.UpdateAnswer
{
    public record UpdateAnswerCommand(
        string AnswerText
    ) : ICommand;
}
