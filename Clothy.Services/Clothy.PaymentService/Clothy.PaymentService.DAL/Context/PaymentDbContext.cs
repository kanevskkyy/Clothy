using Clothy.PaymentService.DAL.Context.Config;
using Clothy.PaymentService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.PaymentService.DAL.Context
{
    public class PaymentDbContext : DbContext
    {
        public PaymentDbContext(DbContextOptions options) : base(options)
        {

        }
        
        public DbSet<PaymentRecordEF> PaymentRecords { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new PaymentRecordConfiguration());

            base.OnModelCreating(modelBuilder);
        }
    }
}
