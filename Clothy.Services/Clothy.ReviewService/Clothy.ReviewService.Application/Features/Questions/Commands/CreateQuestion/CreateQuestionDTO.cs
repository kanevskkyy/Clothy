using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.ReviewService.Application.Features.Questions.Commands.CreateQuestion
{
    public class CreateQuestionDTO
    {
        public Guid ClotheItemId {  get; set; }
        public string QuestionText { get; set; }
    }
}
