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
        [EnumMember(Value = "Awaiting payment")]
        AwaitingPayment = 0,

        [EnumMember(Value = "Processing by managers")]
        Processing = 1,

        [EnumMember(Value = "Shipped")]
        Shipped = 2,

        [EnumMember(Value = "Delivered")]
        Delivered = 3
    }
}