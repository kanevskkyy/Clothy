using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace Clothy.UserService.BLL.Exceptions
{
    public class IdentityOperationException : Exception
    {
        public IEnumerable<IdentityError> Errors { get; }

        public IdentityOperationException(string message, IEnumerable<IdentityError> errors) : base(message)
        {
            Errors = errors;
        }
    }
}
