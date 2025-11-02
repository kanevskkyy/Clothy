using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.OrderService.DAL.FilterDTOs;
using Clothy.OrderService.Domain.Entities;

namespace Clothy.OrderService.DAL.Interfaces
{
    public interface ICityRepository : IGenericRepository<City>
    {
        Task<bool> ExistsByNameAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default);
        Task<(IEnumerable<City> Items, int TotalCount)> GetPagedAsync(CityFilterDTO filter, CancellationToken cancellationToken = default);
    }

}
