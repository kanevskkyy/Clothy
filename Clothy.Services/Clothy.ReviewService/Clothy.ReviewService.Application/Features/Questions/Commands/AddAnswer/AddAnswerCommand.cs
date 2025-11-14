using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.ReviewService.Application.Interfaces.Commands;
using Clothy.ReviewService.Domain.ValueObjects;

namespace Clothy.ReviewService.Application.Features.Questions.Commands.AddAnswer
{
    public record AddAnswerCommand(UserInfo User, string AnswerText) : ICommand<string>;
}
