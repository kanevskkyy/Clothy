using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Clothy.ReviewService.Application.Interfaces.Commands;
using Clothy.ReviewService.Domain.Entities;
using Clothy.ReviewService.Domain.Interfaces;
using Clothy.ReviewService.Domain.ValueObjects;
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
            Question? question = await questionRepository.GetByIdAsync(request.QuestionId, cancellationToken);
            if (question == null) throw new NotFoundException($"Question with ID {request.QuestionId} not found!");

            if (question.User.UserId == request.UserId) throw new ValidationFailedException("You cannot answer your own question");

            UserInfo userInfo = new UserInfo(request.UserId, request.FirstName, request.LastName, request.PhotoUrl);
            Answer answer = new Answer(userInfo, request.AnswerText);

            await questionRepository.AddAnswerAsync(request.QuestionId, answer, cancellationToken);
            return answer;
        }
    }
}
