using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.ReviewService.Domain.Exceptions
{
    public class EmptyValueException : DomainException
    {
        public EmptyValueException(string fieldName) : base($"{fieldName} cannot be empty.")
        {

        }
    }
}
