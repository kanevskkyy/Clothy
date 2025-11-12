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

namespace Clothy.ReviewService.Application.Features.Questions.Commands.DeleteQuestion
{
    public class DeleteQuestionCommandHandler : ICommandHandler<DeleteQuestionCommand>
    {
        private IQuestionRepository questionRepository;

        public DeleteQuestionCommandHandler(IQuestionRepository questionRepository)
        {
            this.questionRepository = questionRepository;
        }

        public async Task<Unit> Handle(DeleteQuestionCommand request, CancellationToken cancellationToken)
        {
            Question? question = await questionRepository.GetByIdAsync(request.QuestionId, cancellationToken);
            if (question == null) throw new NotFoundException($"Question with ID {request.QuestionId} not found!");

            await questionRepository.DeleteAsync(request.QuestionId, cancellationToken);
            return Unit.Value;
        }
    }
}
