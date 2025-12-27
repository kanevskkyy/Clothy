using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Clothy.OrderService.Domain.Entities.AdditionalEntities
{

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum SettlementType
    {
        Village = 0,
        City = 1,
        Urban = 2
    }
}
