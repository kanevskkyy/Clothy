using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.OrderService.DAL.UOW;

namespace Clothy.OrderService.SeedData.Seeders
{
    public interface ISeeder
    {
        Task SeedAsync(IUnitOfWork uow);
    }
}
