using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.Shared.Events.ConsumerService
{
    public interface IEventHandler<T>
    {
        Task HandleAsync(T newEvent, CancellationToken cancellationToken = default);
    }
}
