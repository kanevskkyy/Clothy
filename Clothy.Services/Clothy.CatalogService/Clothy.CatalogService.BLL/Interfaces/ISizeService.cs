using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.CatalogService.BLL.DTOs.SizeDTOs;

namespace Clothy.CatalogService.BLL.Interfaces
{
    public interface ISizeService
    {
        Task<SizeReadDTO> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<List<SizeReadDTO>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<SizeReadDTO> CreateAsync(SizeCreateDTO sizeCreateDTO, CancellationToken cancellationToken = default);
        Task<SizeReadDTO> UpdateAsync(Guid id, SizeUpdateDTO sizeUpdateDTO, CancellationToken cancellationToken = default);
        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
