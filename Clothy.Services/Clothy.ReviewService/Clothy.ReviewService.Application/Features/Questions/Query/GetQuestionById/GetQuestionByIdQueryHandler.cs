using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.ReviewService.Domain.Entities;
using Clothy.ReviewService.Domain.Interfaces;
using Clothy.Shared.Helpers.Exceptions;
using MediatR;

namespace Clothy.ReviewService.Application.Features.Questions.Query.GetQuestionById
{
    public class GetQuestionByIdQueryHandler : IRequestHandler<GetQuestionByIdQuery, Question?>
    {
        private IQuestionRepository questionRepository;

        public GetQuestionByIdQueryHandler(IQuestionRepository questionRepository)
        {
            this.questionRepository = questionRepository;
        }

        public async Task<Question?> Handle(GetQuestionByIdQuery request, CancellationToken cancellationToken)
        {
            Question? question = await questionRepository.GetByIdAsync(request.QuestionId, cancellationToken);
            if (question == null) throw new NotFoundException($"Question with ID {request.QuestionId} not found!");

            return question;
        }
    }
}
