using Clothy.PaymentService.DAL.Context;
using Clothy.PaymentService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Clothy.PaymentService.BLL.Services
{
    /// <summary>
    /// Compare-and-set updates of a payment record. Status and FraudSync are EF concurrency tokens, so
    /// SaveChanges fails when another webhook changed them after we read the row. Then the row is re-read
    /// and the decision is made again on fresh values.
    /// </summary>
    public class PaymentRecordUpdater
    {
        private const int MAX_ATTEMPTS = 5;

        private readonly PaymentDbContext dbContext;

        public PaymentRecordUpdater(PaymentDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        /// <summary>
        /// Reads the payment, lets <paramref name="apply"/> change it and decide what to do next, saves.
        /// <paramref name="apply"/> may run several times and must have no side effects besides changing the entity.
        /// </summary>
        /// <returns>The decision of the run that was saved; default when the payment does not exist.</returns>
        public async Task<TResult?> UpdateAsync<TResult>(Guid paymentId, Func<PaymentRecordEF, TResult> apply, CancellationToken cancellationToken)
        {
            for (int attempt = 1; ; attempt++)
            {
                PaymentRecordEF? payment = await dbContext.PaymentRecords.FirstOrDefaultAsync(p => p.Id == paymentId, cancellationToken);
                if (payment == null) return default;

                // an already tracked entity is not refreshed by the query: read the current row explicitly
                await dbContext.Entry(payment).ReloadAsync(cancellationToken);

                TResult result = apply(payment);
                try
                {
                    await dbContext.SaveChangesAsync(cancellationToken);
                    return result;
                }
                catch (DbUpdateConcurrencyException) when (attempt < MAX_ATTEMPTS)
                {
                    // another webhook won the race: decide again on the new values
                }
            }
        }
    }
}
