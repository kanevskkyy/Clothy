using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.ReviewService.Application.Features.Questions.Commands.CreateQuestion;
using FluentValidation;

namespace Clothy.ReviewService.Application.Validations.Questions
{
    public class CreateQuestionDTOValidator : AbstractValidator<CreateQuestionDTO>
    {
        public CreateQuestionDTOValidator()
        {
            RuleFor(x => x.ClotheItemId)
                .NotEmpty().WithMessage("ClotheItemId is required");

            RuleFor(x => x.QuestionText)
                .NotEmpty().WithMessage("Question text is required")
                .MinimumLength(5).WithMessage("Question text must be at least 5 characters")
                .MaximumLength(500).WithMessage("Question text must not exceed 500 characters");
        }
    }
}
