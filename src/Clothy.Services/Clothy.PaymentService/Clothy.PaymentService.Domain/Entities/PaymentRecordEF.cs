using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.PaymentService.Domain.Entities
{
    public class PaymentRecordEF
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public Guid UserId { get; set; }
        public decimal Price { get; set; }
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Card;
        public string? TransactionId { get; set; }  
        public DateTime CreatedAt { get; set; } = DateTime.Now.ToUniversalTime();
        public DateTime? UpdatedAt { get; set; }
        /// <summary>Creation time of the Stripe dispute (the time of the fraud label).</summary>
        public DateTime? DisputedAt { get; set; }
        public FraudSyncState FraudSync { get; set; } = FraudSyncState.None;
    }
}
