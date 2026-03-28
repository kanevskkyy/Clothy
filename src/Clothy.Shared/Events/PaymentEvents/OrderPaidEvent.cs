using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.Shared.Events.PaymentEvents
{
    public record OrderPaidEvent : BaseEvent
    {
        public Guid OrderId {  get; init; }
    }
}
