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

namespace Clothy.ReviewService.Application.Features.Questions.Commands.DeleteAnswer
{
    public class DeleteAnswerToQuestionCommandHandler : ICommandHandler<DeleteAnswerToQuestionCommand>
    {
        private IQuestionRepository questionRepository;

        public DeleteAnswerToQuestionCommandHandler(IQuestionRepository questionRepository)
        {
            this.questionRepository = questionRepository;
        }

        public async Task<Unit> Handle(DeleteAnswerToQuestionCommand request, CancellationToken cancellationToken)
        {
            Question? question = await questionRepository.GetByIdAsync(request.QuestionId, cancellationToken);
            if (question == null) throw new NotFoundException($"Question with ID {request.QuestionId} not found!");

            await questionRepository.DeleteAnswerAsync(request.QuestionId, request.AnswerId, cancellationToken);
            return Unit.Value;
        }
    }
}
