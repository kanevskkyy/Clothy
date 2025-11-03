using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.Shared.Helpers.Exceptions
{
    public class ValidationFailedException : Exception
    {
        public ValidationFailedException(string message) : base(message) 
        {
            
        }
    }
}
