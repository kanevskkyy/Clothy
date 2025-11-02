using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.ReviewService.Domain.Entities;
using Clothy.ReviewService.Domain.Helpers;
using Clothy.ReviewService.Domain.Interfaces.Services;
using Clothy.Shared.Helpers;
using MediatR;

namespace Clothy.ReviewService.Application.Features.Questions.Query.GetQuestions
{
    public class GetQuestionsQueryHandler : IRequestHandler<GetQuestionsQuery, PagedList<Question>>
    {
        private IQuestionService questionService;

        public GetQuestionsQueryHandler(IQuestionService questionService)
        {
            this.questionService = questionService;
        }

        public async Task<PagedList<Question>> Handle(GetQuestionsQuery request, CancellationToken cancellationToken)
        {
            return await questionService.GetQuestionsAsync(request.QueryParameters, cancellationToken);
        }
    }
}
