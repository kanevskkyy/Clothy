using Clothy.PaymentService.Domain.Entities;

namespace Clothy.PaymentService.BLL.DTOs
{
    public class OrderPaymentStatusDTO
    {
        public Guid OrderId { get; set; }
        public PaymentStatus Status { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
