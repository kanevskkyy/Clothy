using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.ReviewService.Domain.Entities.QueryParameters;
using Clothy.ReviewService.Domain.Entities;
using Clothy.ReviewService.Domain.Helpers;
using MediatR;

namespace Clothy.ReviewService.Application.Features.Questions.Query.GetQuestions
{
    public class GetQuestionsQuery : IRequest<PagedList<Question>>
    {
        public QuestionQueryParameters QueryParameters { get; }

        public GetQuestionsQuery(QuestionQueryParameters queryParameters)
        {
            QueryParameters = queryParameters;
        }
    }
}
