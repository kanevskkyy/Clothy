using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.ReviewService.Domain.Exceptions
{
    public class EmptyQuestionException : DomainException
    {
        public EmptyQuestionException() : base("Question text cannot be empty.") 
        {

        }
    }
}
