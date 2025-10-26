using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.CatalogService.BLL.DTOs.MaterialDTOs;

namespace Clothy.CatalogService.BLL.Interfaces
{
    public interface IMaterialService
    {
        Task<MaterialReadDTO> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<List<MaterialReadDTO>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<List<MaterialWithCountDTO>> GetAllWithCountAsync(CancellationToken cancellationToken = default);
        Task<MaterialReadDTO> CreateAsync(MaterialCreateDTO materialCreateDTO, CancellationToken cancellationToken = default);
        Task<MaterialReadDTO> UpdateAsync(Guid id, MaterialUpdateDTO materialUpdateDTO, CancellationToken cancellationToken = default);
        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
