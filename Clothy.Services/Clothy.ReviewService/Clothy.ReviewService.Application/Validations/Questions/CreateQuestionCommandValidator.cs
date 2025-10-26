using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.ReviewService.Application.Features.Questions.Commands.CreateQuestion;
using Clothy.ReviewService.Application.Validations.Additional;
using FluentValidation;

namespace Clothy.ReviewService.Application.Validations.Questions
{
    public class CreateQuestionCommandValidator : AbstractValidator<CreateQuestionCommand>
    {
        public CreateQuestionCommandValidator()
        {
            RuleFor(q => q.ClotheItemId)
                .NotEmpty().WithMessage("ClotheItemId is required.");

            RuleFor(q => q.User)
                .NotNull().WithMessage("User is required.")
                .SetValidator(new UserInfoValidator());

            RuleFor(q => q.QuestionText)
                .NotEmpty().WithMessage("Question text is required.")
                .MinimumLength(5).WithMessage("Question text must be at least 5 characters long.")
                .MaximumLength(500).WithMessage("Question text cannot exceed 500 characters.");
        }
    }
}
