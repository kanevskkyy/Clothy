using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.ReviewService.Application.Interfaces.Commands;
using Clothy.ReviewService.Domain.Interfaces.Services;
using MediatR;

namespace Clothy.ReviewService.Application.Features.Questions.Commands.DeleteAnswer
{
    public class DeleteAnswerToQuestionCommandHandler : ICommandHandler<DeleteAnswerToQuestionCommand>
    {
        private IQuestionService questionService;

        public DeleteAnswerToQuestionCommandHandler(IQuestionService questionService)
        {
            this.questionService = questionService;
        }

        public async Task<Unit> Handle(DeleteAnswerToQuestionCommand request, CancellationToken cancellationToken)
        {
            await questionService.DeleteAnswerAsync(request.QuestionId, request.AnswerId, cancellationToken);
            return Unit.Value;
        }
    }
}
