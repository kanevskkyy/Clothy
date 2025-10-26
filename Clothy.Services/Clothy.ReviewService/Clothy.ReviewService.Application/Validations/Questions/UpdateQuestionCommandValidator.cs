using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.ReviewService.Application.Features.Questions.Commands.UpdateQuestion;
using FluentValidation;

namespace Clothy.ReviewService.Application.Validations.Questions
{
    public class UpdateQuestionCommandValidator : AbstractValidator<UpdateQuestionCommand>
    {
        public UpdateQuestionCommandValidator()
        {
            RuleFor(q => q.QuestionText)
                .NotEmpty().WithMessage("Question text is required.")
                .MinimumLength(5).WithMessage("Question text must be at least 5 characters long.")
                .MaximumLength(500).WithMessage("Question text cannot exceed 500 characters.");
        }
    }
}
