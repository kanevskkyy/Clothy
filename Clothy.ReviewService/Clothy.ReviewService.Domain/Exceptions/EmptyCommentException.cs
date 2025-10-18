using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.ReviewService.Domain.Exceptions
{
    public class EmptyCommentException : DomainException
    {
        public EmptyCommentException() : base("Comment cannot be empty.") 
        {

        }
    }
}
