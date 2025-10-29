using Clothy.Aggregator.DTOs.Questions;
using Clothy.Aggregator.DTOs.Reviews;
using Clothy.CatalogService.BLL.DTOs.ClotheDTOs;

namespace Clothy.Aggregator.DTOs.ClotheItem
{
    public class ClotheDetailFullDTO
    {
        public ClotheDetailDTO? ClotheDetailDTO { get; set; }
        public List<ReviewResponseDTO> Reviews { get; set; } = new();
        public ReviewStatisticsDTO? Statistics { get; set; }
        public List<QuestionResponseDTO> Questions { get; set; } = new();
    }
}
