using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.ReviewService.Application.Features.Questions.Commands.UpdateQuestion;
using FluentValidation;

namespace Clothy.ReviewService.Application.Validations.Questions
{
    public class UpdateQuestionDTOValidator : AbstractValidator<UpdateQuestionDTO>
    {
        public UpdateQuestionDTOValidator()
        {
            RuleFor(x => x.QuestionText)
                .NotEmpty().WithMessage("Question text is required")
                .MaximumLength(500).WithMessage("Question text must not exceed 500 characters")
                .MinimumLength(5).WithMessage("Question text must be at least 5 characters");
        }
    }
}
