using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.ReviewService.Application.Interfaces.Commands;
using Clothy.ReviewService.Domain.Entities;
using Clothy.ReviewService.Domain.Interfaces.Services;

namespace Clothy.ReviewService.Application.Features.Questions.Commands.CreateQuestion
{
    public class CreateQuestionCommandHandler : ICommandHandler<CreateQuestionCommand, Guid>
    {
        private IQuestionService questionService;

        public CreateQuestionCommandHandler(IQuestionService questionService)
        {
            this.questionService = questionService;
        }

        public async Task<Guid> Handle(CreateQuestionCommand request, CancellationToken cancellationToken)
        {
            Question question = await questionService.AddQuestionAsync(
                new Question(request.ClotheItemId, request.User, request.QuestionText),
                cancellationToken
            );

            return Guid.Parse(question.Id);
        }
    }
}
