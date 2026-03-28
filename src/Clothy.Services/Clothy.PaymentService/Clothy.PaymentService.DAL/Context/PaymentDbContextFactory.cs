using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.PaymentService.DAL.Context
{
    public class PaymentDbContextFactory : IDesignTimeDbContextFactory<PaymentDbContext>
    {
        public PaymentDbContext CreateDbContext(string[] args)
        {
            const string CONNECTION_STRING = "Host=localhost;Port=5432;Database=clothy_payment;Username=postgres;Password=postgres";

            DbContextOptionsBuilder<PaymentDbContext> dbContextOptionsBuilder = new DbContextOptionsBuilder<PaymentDbContext>();
            dbContextOptionsBuilder.UseNpgsql(CONNECTION_STRING);

            return new PaymentDbContext(dbContextOptionsBuilder.Options);
        }
    }
}
