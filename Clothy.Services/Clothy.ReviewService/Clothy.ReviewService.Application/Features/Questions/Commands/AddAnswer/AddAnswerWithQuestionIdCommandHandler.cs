using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.ReviewService.Application.Interfaces.Commands;
using Clothy.ReviewService.Domain.Entities;
using Clothy.ReviewService.Domain.Interfaces.Services;

namespace Clothy.ReviewService.Application.Features.Questions.Commands.AddAnswer
{
    public class AddAnswerWithQuestionIdCommandHandler : ICommandHandler<AddAnswerWithQuestionIdCommand, Answer>
    {
        private IQuestionService questionService;

        public AddAnswerWithQuestionIdCommandHandler(IQuestionService questionService)
        {
            this.questionService = questionService;
        }

        public async Task<Answer> Handle(AddAnswerWithQuestionIdCommand request, CancellationToken cancellationToken)
        {
            Answer answer = new Answer(request.User, request.AnswerText);
            await questionService.AddAnswerAsync(request.QuestionId, answer, cancellationToken);
            
            return answer;
        }
    }
}
