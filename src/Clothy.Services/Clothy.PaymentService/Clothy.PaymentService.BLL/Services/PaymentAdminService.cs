using Clothy.PaymentService.BLL.DTOs;
using Clothy.PaymentService.DAL.Context;
using Clothy.PaymentService.Domain.Entities;
using Clothy.Shared.Helpers.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Clothy.PaymentService.BLL.Services
{
    public interface IPaymentAdminService
    {
        /// <summary>Payment status per order (for the admin orders page).</summary>
        Task<List<OrderPaymentStatusDTO>> GetStatusesByOrdersAsync(IReadOnlyCollection<Guid> orderIds, CancellationToken cancellationToken);
    }

    public class PaymentAdminService : IPaymentAdminService
    {
        public const int MAX_ORDERS = 100;

        private readonly PaymentDbContext dbContext;

        public PaymentAdminService(PaymentDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<List<OrderPaymentStatusDTO>> GetStatusesByOrdersAsync(IReadOnlyCollection<Guid> orderIds, CancellationToken cancellationToken)
        {
            if (orderIds.Count > MAX_ORDERS) throw new ValidationFailedException($"No more than {MAX_ORDERS} orders per request!");
            if (orderIds.Count == 0) return new List<OrderPaymentStatusDTO>();

            List<PaymentRecordEF> records = await dbContext.PaymentRecords
                .AsNoTracking()
                .Where(p => orderIds.Contains(p.OrderId))
                .ToListAsync(cancellationToken);

            // an order can have several attempts: the paid (or disputed) one wins, otherwise the latest attempt
            return records
                .GroupBy(p => p.OrderId)
                .Select(g => g
                    .OrderByDescending(p => p.Status != PaymentStatus.Pending)
                    .ThenByDescending(p => p.UpdatedAt ?? p.CreatedAt)
                    .First())
                .Select(p => new OrderPaymentStatusDTO
                {
                    OrderId = p.OrderId,
                    Status = p.Status,
                    PaymentMethod = p.PaymentMethod,
                    UpdatedAt = p.UpdatedAt ?? p.CreatedAt
                })
                .ToList();
        }
    }
}
