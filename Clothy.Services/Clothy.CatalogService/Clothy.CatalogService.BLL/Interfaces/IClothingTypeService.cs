using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.CatalogService.BLL.DTOs.ClothingTypeDTOs;

namespace Clothy.CatalogService.BLL.Interfaces
{
    public interface IClothingTypeService
    {
        Task<ClothingTypeReadDTO> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<List<ClothingTypeReadDTO>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<ClothingTypeReadDTO> CreateAsync(ClothingTypeCreateDTO clothingTypeCreateDTO, CancellationToken cancellationToken = default);
        Task<ClothingTypeReadDTO> UpdateAsync(Guid id, ClothingTypeUpdateDTO clothingTypeUpdateDTO, CancellationToken cancellationToken = default);
        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
