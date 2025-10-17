using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.CatalogService.BLL.Exceptions
{
    public class InvalidMaterialPercentageException : Exception
    {
        public InvalidMaterialPercentageException(string message) : base(message)
        {

        }
    }
}
