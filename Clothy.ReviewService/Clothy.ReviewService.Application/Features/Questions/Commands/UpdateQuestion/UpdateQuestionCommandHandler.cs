using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.ReviewService.Application.Interfaces.Commands;
using Clothy.ReviewService.Domain.Interfaces.Services;
using MediatR;

namespace Clothy.ReviewService.Application.Features.Questions.Commands.UpdateQuestion
{
    public class UpdateQuestionCommandHandler : ICommandHandler<UpdateQuestionCommand>
    {
        private IQuestionService questionService;

        public UpdateQuestionCommandHandler(IQuestionService questionService)
        {
            this.questionService = questionService;
        }

        public async Task<Unit> Handle(UpdateQuestionCommand request, CancellationToken cancellationToken)
        {
            await questionService.UpdateQuestionAsync(request.QuestionId.ToString(), request.QuestionText, cancellationToken);
            return Unit.Value;
        }
    }
}
