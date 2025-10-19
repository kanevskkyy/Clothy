using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.ReviewService.Application.Interfaces.Commands;
using Clothy.ReviewService.Domain.Interfaces.Services;
using MediatR;

namespace Clothy.ReviewService.Application.Features.Questions.Commands.DeleteQuestion
{
    public class DeleteQuestionCommandHandler : ICommandHandler<DeleteQuestionCommand>
    {
        private IQuestionService questionService;

        public DeleteQuestionCommandHandler(IQuestionService questionService)
        {
            this.questionService = questionService;
        }

        public async Task<Unit> Handle(DeleteQuestionCommand request, CancellationToken cancellationToken)
        {
            await questionService.DeleteQuestionAsync(request.QuestionId.ToString(), cancellationToken);
            return Unit.Value;
        }
    }
}
