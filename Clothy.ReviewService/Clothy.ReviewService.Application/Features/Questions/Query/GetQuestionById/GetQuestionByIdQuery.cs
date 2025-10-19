using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.ReviewService.Domain.Entities;
using MediatR;

namespace Clothy.ReviewService.Application.Features.Questions.Query.GetQuestionById
{
    public class GetQuestionByIdQuery : IRequest<Question?>
    {
        public string QuestionId { get; }

        public GetQuestionByIdQuery(string questionId)
        {
            QuestionId = questionId;
        }
    }
}
