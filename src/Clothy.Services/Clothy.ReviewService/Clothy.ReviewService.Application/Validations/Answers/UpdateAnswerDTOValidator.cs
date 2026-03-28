using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.ReviewService.Application.Features.Questions.Commands.UpdateAnswer;
using FluentValidation;

namespace Clothy.ReviewService.Application.Validations.Answers
{
    public class UpdateAnswerDTOValidator : AbstractValidator<UpdateAnswerDTO>
    {
        public UpdateAnswerDTOValidator()
        {
            RuleFor(x => x.AnswerText)
                .NotEmpty().WithMessage("Answer text is required")
                .MaximumLength(500).WithMessage("Answer text must not exceed 500 characters")
                .MinimumLength(2).WithMessage("Answer text must be at least 2 characters");
        }
    }
}
