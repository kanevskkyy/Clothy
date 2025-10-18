using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.ReviewService.Domain.Exceptions
{
    public class EmptyAnswerException : DomainException
    {
        public EmptyAnswerException() : base("Answer text cannot be empty.") 
        {
            
        }
    }
}
