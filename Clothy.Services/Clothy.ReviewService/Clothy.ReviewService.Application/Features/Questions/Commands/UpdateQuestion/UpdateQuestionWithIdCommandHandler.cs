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

namespace Clothy.ReviewService.Application.Features.Questions.Commands.UpdateQuestion
{
    public class UpdateQuestionWithIdCommandHandler : ICommandHandler<UpdateQuestionWithIdCommand>
    {
        private IQuestionRepository questionRepository;

        public UpdateQuestionWithIdCommandHandler(IQuestionRepository questionRepository)
        {
            this.questionRepository = questionRepository;
        }

        public async Task<Unit> Handle(UpdateQuestionWithIdCommand request, CancellationToken cancellationToken)
        {
            Question? question = await questionRepository.GetByIdAsync(request.QuestionId, cancellationToken);
            if (question == null) throw new NotFoundException($"Question with ID {request.QuestionId} not found!");

            question.UpdateQuestion(request.QuestionText);
            await questionRepository.UpdateQuestionAsync(question, cancellationToken);
            return Unit.Value;
        }
    }
}
