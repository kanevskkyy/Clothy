using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.ReviewService.Domain.Entities;
using Clothy.ReviewService.Domain.Helpers;
using Clothy.ReviewService.Domain.Interfaces;
using Clothy.Shared.Helpers;
using MediatR;

namespace Clothy.ReviewService.Application.Features.Questions.Query.GetQuestions
{
    public class GetQuestionsQueryHandler : IRequestHandler<GetQuestionsQuery, PagedList<Question>>
    {
        private IQuestionRepository questionRepository;

        public GetQuestionsQueryHandler(IQuestionRepository questionRepository)
        {
            this.questionRepository = questionRepository;
        }

        public async Task<PagedList<Question>> Handle(GetQuestionsQuery request, CancellationToken cancellationToken)
        {
            return await questionRepository.GetQuestionsAsync(request.QueryParameters, cancellationToken);
        }
    }
}
