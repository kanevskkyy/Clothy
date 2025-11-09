using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.OrderService.DAL.FilterDTOs;
using Clothy.OrderService.Domain.Entities;

namespace Clothy.OrderService.DAL.Interfaces
{
    public interface IPickupPointRepository : IGenericRepository<PickupPoints>
    {
        Task<(IEnumerable<PickupPoints>, int totalCount)> GetPagedAsync(PickupPointFilterDTO filterDTO, CancellationToken cancellationToken = default);
        Task<bool> ExistsByAddressAndProviderIdAsync(string address, Guid deliveryProviderId, Guid? excludeId = null, CancellationToken cancellationToken = default);
    }
}
