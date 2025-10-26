using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.CatalogService.BLL.DTOs.ColorDTOs;

namespace Clothy.CatalogService.BLL.Interfaces
{
    public interface IColorService
    {
        Task<ColorReadDTO> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<List<ColorReadDTO>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<List<ColorWithCountDTO>> GetAllWithCountAsync(CancellationToken cancellationToken = default);
        Task<ColorReadDTO> CreateAsync(ColorCreateDTO colorCreateDTO, CancellationToken cancellationToken = default);
        Task<ColorReadDTO> UpdateAsync(Guid id, ColorUpdateDTO colorUpdateDTO, CancellationToken cancellationToken = default);
        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
