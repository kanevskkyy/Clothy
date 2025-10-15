using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.CatalogService.BLL.DTOs.BrandDTOs;

namespace Clothy.CatalogService.BLL.Interfaces
{
    public interface IBrandService
    {
        Task<List<BrandReadDTO>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<BrandReadDTO> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<BrandReadDTO> CreateAsync(BrandCreateDTO dto, CancellationToken cancellationToken = default);
        Task<BrandReadDTO> UpdateAsync(Guid id, BrandUpdateDTO dto, CancellationToken cancellationToken = default);
        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
