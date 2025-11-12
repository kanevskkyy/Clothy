using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.ReviewService.Application.Interfaces.Commands;
using Clothy.ReviewService.Domain.Entities;
using Clothy.ReviewService.Domain.Interfaces;
using Clothy.Shared.Helpers.Exceptions;
using MediatR;

namespace Clothy.ReviewService.Application.Features.Questions.Commands.UpdateAnswer
{
    public class UpdateAnswerWithIdsCommandHandler : ICommandHandler<UpdateAnswerWithIdsCommand>
    {
        private IQuestionRepository questionRepository;

        public UpdateAnswerWithIdsCommandHandler(IQuestionRepository questionRepository)
        {
            this.questionRepository = questionRepository;
        }

        public async Task<Unit> Handle(UpdateAnswerWithIdsCommand request, CancellationToken cancellationToken)
        {
            Question? question = await questionRepository.GetByIdAsync(request.QuestionId, cancellationToken);
            if (question == null) throw new NotFoundException($"Question with ID {request.QuestionId} not found!");

            Answer? answer = question.Answers.FirstOrDefault(tempAnswer => tempAnswer.Id == request.AnswerId);
            if (answer == null) throw new NotFoundException($"Answer with ID {request.AnswerId} not found!");

            answer.UpdateAnswer(request.AnswerText);
            await questionRepository.UpdateAnswerAsync(request.QuestionId, answer, cancellationToken);
            return Unit.Value;
        }
    }
}
