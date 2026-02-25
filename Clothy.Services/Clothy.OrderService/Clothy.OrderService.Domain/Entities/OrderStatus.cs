using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Clothy.OrderService.Domain.Entities
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum OrderStatus
    {
        AwaitingPayment = 0,
        Processing = 1,
        Shipped = 2,
        Delivered = 3
    }
}