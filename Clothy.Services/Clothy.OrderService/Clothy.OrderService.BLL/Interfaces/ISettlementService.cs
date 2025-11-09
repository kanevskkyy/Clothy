using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.OrderService.BLL.DTOs.SettlementDTOs;
using Clothy.OrderService.DAL.FilterDTOs;
using Clothy.OrderService.Domain.Entities;
using Clothy.Shared.Helpers;

namespace Clothy.OrderService.BLL.Interfaces
{
    public interface ISettlementService
    {
        Task<PagedList<SettlementReadDTO>> GetPagedAsync(SettlementFilterDTO settlementFilterDTO, CancellationToken cancellationToken = default);
        Task<SettlementReadDTO> GetByIdAsync(Guid id,  CancellationToken cancellationToken = default);
        Task<SettlementReadDTO> CreateAsync(SettlementCreateDTO settlementCreateDTO, CancellationToken cancellationToken = default);
        Task<SettlementReadDTO> UpdateAsync(Guid id, SettlementUpdateDTO settlementUpdateDTO, CancellationToken cancellationToken = default);
        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
