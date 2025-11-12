using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.ReviewService.Application.Interfaces.Commands;
using Clothy.ReviewService.Domain.Entities;
using Clothy.ReviewService.Domain.Interfaces;
using Clothy.Shared.Helpers.Exceptions;

namespace Clothy.ReviewService.Application.Features.Questions.Commands.AddAnswer
{
    public class AddAnswerWithQuestionIdCommandHandler : ICommandHandler<AddAnswerWithQuestionIdCommand, Answer>
    {
        private IQuestionRepository questionRepository;

        public AddAnswerWithQuestionIdCommandHandler(IQuestionRepository questionRepository)
        {
            this.questionRepository = questionRepository;
        }

        public async Task<Answer> Handle(AddAnswerWithQuestionIdCommand request, CancellationToken cancellationToken)
        {
            Answer answer = new Answer(request.User, request.AnswerText);
            Question? question = await questionRepository.GetByIdAsync(request.QuestionId, cancellationToken);
            if (question == null) throw new NotFoundException($"Question with ID {request.QuestionId} not found!");

            await questionRepository.AddAnswerAsync(request.QuestionId, answer, cancellationToken);

            return answer;
        }
    }
}
