using Clothy.Aggregator.DTOs.Reviews;

namespace Clothy.Aggregator.DTOs.Questions
{
    public class QuestionResponseDTO
    {
        public string? Id { get; set; } 
        public UserInfoDTO? User { get; set; } 
        public string? QuestionText { get; set; } 
        public List<AnswerResponseDTO> Answers { get; set; } = new();
    }
}
