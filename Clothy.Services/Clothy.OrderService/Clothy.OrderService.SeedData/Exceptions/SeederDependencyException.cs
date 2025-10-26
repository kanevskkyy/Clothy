using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.OrderService.SeedData.Exceptions
{
    public class SeederDependencyException : Exception
    {
        public SeederDependencyException(string message) : base(message)
        {

        }
    }
}
