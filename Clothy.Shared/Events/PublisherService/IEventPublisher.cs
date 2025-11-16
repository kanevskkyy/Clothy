using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.Shared.Events.PublisherService
{
    public interface IEventPublisher
    {
        Task PublishAsync<TEvent>(TEvent @event, string exchangeName, string routingKey) where TEvent : class;
    }
}
