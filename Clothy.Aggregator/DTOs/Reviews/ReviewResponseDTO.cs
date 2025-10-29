namespace Clothy.Aggregator.DTOs.Reviews
{
    public class ReviewResponseDTO
    {
        public string? Id { get; set; } 
        public UserInfoDTO? User { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
