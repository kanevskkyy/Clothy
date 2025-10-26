using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.ReviewService.Application.Interfaces.Commands;
using Clothy.ReviewService.Domain.Interfaces.Services;
using MediatR;

namespace Clothy.ReviewService.Application.Features.Questions.Commands.UpdateAnswer
{
    public class UpdateAnswerWithIdsCommandHandler : ICommandHandler<UpdateAnswerWithIdsCommand>
    {
        private IQuestionService questionService;

        public UpdateAnswerWithIdsCommandHandler(IQuestionService questionService)
        {
            this.questionService = questionService;
        }

        public async Task<Unit> Handle(UpdateAnswerWithIdsCommand request, CancellationToken cancellationToken)
        {
            await questionService.UpdateAnswerAsync(request.QuestionId, request.AnswerId, request.AnswerText, cancellationToken);
            return Unit.Value;
        }
    }
}
