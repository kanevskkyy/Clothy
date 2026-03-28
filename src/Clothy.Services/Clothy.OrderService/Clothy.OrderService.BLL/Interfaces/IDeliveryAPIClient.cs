using Clothy.OrderService.BLL.DTOs.APIClientDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.OrderService.BLL.Interfaces
{
    public interface IDeliveryAPIClient
    {
        Task<List<RegionDTO>> GetAreasAsync(CancellationToken cancellationToken = default);
        Task<List<SettlementDTO>> GetSettlementsByAreaRefAsync(string areaRef, CancellationToken cancellationToken = default);
        Task<List<PickupPointDTO>> GetWarehousesByCityRefAsync(string cityRef, CancellationToken cancellationToken = default);
    }
}
