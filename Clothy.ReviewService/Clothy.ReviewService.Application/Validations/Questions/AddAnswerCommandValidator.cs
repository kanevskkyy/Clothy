using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.ReviewService.Application.Features.Questions.Commands.AddAnswer;
using Clothy.ReviewService.Application.Validations.Additional;
using FluentValidation;

namespace Clothy.ReviewService.Application.Validations.Questions
{
    public class AddAnswerCommandValidator : AbstractValidator<AddAnswerCommand>
    {
        public AddAnswerCommandValidator()
        {
            RuleFor(a => a.User)
                .NotNull().WithMessage("User is required.")
                .SetValidator(new UserInfoValidator());

            RuleFor(a => a.AnswerText)
                .NotEmpty().WithMessage("Answer text is required.")
                .MinimumLength(2).WithMessage("Answer text must be at least 2 characters long.")
                .MaximumLength(500).WithMessage("Answer text cannot exceed 500 characters.");
        }
    }
}
