using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.ReviewService.Domain.Entities;
using Clothy.ReviewService.Domain.Interfaces.Services;
using MediatR;

namespace Clothy.ReviewService.Application.Features.Questions.Query.GetQuestionById
{
    public class GetQuestionByIdQueryHandler : IRequestHandler<GetQuestionByIdQuery, Question?>
    {
        private IQuestionService questionService;

        public GetQuestionByIdQueryHandler(IQuestionService questionService)
        {
            this.questionService = questionService;
        }

        public async Task<Question?> Handle(GetQuestionByIdQuery request, CancellationToken cancellationToken)
        {
            return await questionService.GetQuestionByIdAsync(request.QuestionId, cancellationToken);
        }
    }
}
