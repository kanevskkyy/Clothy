using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.CatalogService.BLL.DTOs.TagDTOs;

namespace Clothy.CatalogService.BLL.Interfaces
{
    public interface ITagService
    {
        Task<TagReadDTO> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<List<TagReadDTO>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<List<TagWithCountDTO>> GetAllWithCountAsync(CancellationToken cancellationToken = default);
        Task<TagReadDTO> CreateAsync(TagCreateDTO tagCreateDTO, CancellationToken cancellationToken = default);
        Task<TagReadDTO> UpdateAsync(Guid id, TagUpdateDTO tagUpdateDTO, CancellationToken cancellationToken = default);
        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
