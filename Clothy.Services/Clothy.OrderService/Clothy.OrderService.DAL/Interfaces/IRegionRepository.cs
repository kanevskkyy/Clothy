using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.OrderService.DAL.FilterDTOs;
using Clothy.OrderService.Domain.Entities;

namespace Clothy.OrderService.DAL.Interfaces
{
    public interface IRegionRepository : IGenericRepository<Region>
    {
        Task<(IEnumerable<Region> Items, int TotalCount)> GetPagedAsync(RegionFilterDTO filter, CancellationToken cancellationToken = default);
        Task<bool> ExistByNameAndCityIdAsync(string name, Guid cityId, Guid? excludeId = null, CancellationToken cancellationToken = default);
    }
}
