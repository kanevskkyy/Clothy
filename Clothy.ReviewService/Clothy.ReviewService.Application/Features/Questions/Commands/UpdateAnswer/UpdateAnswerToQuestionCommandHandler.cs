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
    public class UpdateAnswerToQuestionCommandHandler : ICommandHandler<UpdateAnswerToQuestionCommand>
    {
        private IQuestionService questionService;

        public UpdateAnswerToQuestionCommandHandler(IQuestionService questionService)
        {
            this.questionService = questionService;
        }

        public async Task<Unit> Handle(UpdateAnswerToQuestionCommand request, CancellationToken cancellationToken)
        {
            await questionService.UpdateAnswerAsync(request.QuestionId.ToString(), request.AnswerId.ToString(), request.AnswerText, cancellationToken);
            return Unit.Value;
        }
    }
}
