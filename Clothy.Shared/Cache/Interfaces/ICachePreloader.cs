using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.Shared.Cache.Interfaces
{
    public interface ICachePreloader
    {
        Task PreloadAsync(CancellationToken cancellationToken);
    }
}
