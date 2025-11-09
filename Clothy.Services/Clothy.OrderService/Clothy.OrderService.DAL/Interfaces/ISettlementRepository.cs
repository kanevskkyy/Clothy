using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.OrderService.DAL.FilterDTOs;
using Clothy.OrderService.Domain.Entities;

namespace Clothy.OrderService.DAL.Interfaces
{
    public interface ISettlementRepository : IGenericRepository<Settlement>
    {
        Task<(IEnumerable<Settlement>, int totalCount)> GetPagedAsync(SettlementFilterDTO settlementFilterDTO, CancellationToken cancellationToken = default);
        Task<bool> ExistsByNameAndRegionIdAsync(string name, Guid regionId, Guid? excludeId = null, CancellationToken cancellationToken = default);
    }
}
