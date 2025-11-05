using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.Aggregator.Aggregate.DTOs.ClotheItem
{
    public class ClotheDetailFullDTO
    {
        public ClotheDetailGrpcResponse? ClotheDetailDTO { get; set; }
        public List<ReviewGrpcResponse> Reviews { get; set; } = new();
        public ReviewStatisticGrpcResponse? Statistics { get; set; }
        public List<QuestionGrpcResponse> Questions { get; set; } = new();
    }
}
