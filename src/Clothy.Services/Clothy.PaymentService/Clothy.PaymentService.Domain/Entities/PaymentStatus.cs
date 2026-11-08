using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Clothy.PaymentService.Domain.Entities
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum PaymentStatus
    {
        Pending,
        Paid,
        // Stripe disputes (added at the end: values are stored as int, existing numbers must not change)
        Disputed,
        DisputeWon,
        DisputeLost,
    }
}
