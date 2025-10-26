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
    public class UpdateQuestionWithIdCommandHandler : ICommandHandler<UpdateQuestionWithIdCommand>
    {
        private IQuestionService questionService;

        public UpdateQuestionWithIdCommandHandler(IQuestionService questionService)
        {
            this.questionService = questionService;
        }

        public async Task<Unit> Handle(UpdateQuestionWithIdCommand request, CancellationToken cancellationToken)
        {
            await questionService.UpdateQuestionAsync(request.QuestionId, request.QuestionText, cancellationToken);
            return Unit.Value;
        }
    }
}
