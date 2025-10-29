using Clothy.Aggregator.DTOs.Reviews;

namespace Clothy.Aggregator.DTOs.Questions
{
    public class AnswerResponseDTO
    {
        public string? Id { get; set; } 
        public UserInfoDTO? User { get; set; } 
        public string? AnswerText { get; set; }
    }
}
