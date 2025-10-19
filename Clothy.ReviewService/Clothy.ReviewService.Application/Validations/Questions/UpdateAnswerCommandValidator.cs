using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.ReviewService.Application.Features.Questions.Commands.UpdateAnswer;
using FluentValidation;

namespace Clothy.ReviewService.Application.Validations.Questions
{
    public class UpdateAnswerCommandValidator : AbstractValidator<UpdateAnswerCommand>
    {
        public UpdateAnswerCommandValidator()
        {
            RuleFor(a => a.AnswerText)
                .NotEmpty().WithMessage("Answer text is required.")
                .MinimumLength(2).WithMessage("Answer text must be at least 2 characters long.")
                .MaximumLength(500).WithMessage("Answer text cannot exceed 500 characters.");
        }
    }
}
