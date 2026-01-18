using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.CatalogService.BLL.DTOs.CollectionDTOs;

namespace Clothy.CatalogService.BLL.Interfaces
{
    public interface ICollectionService
    {
        Task<CollectionReadDTO> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<List<CollectionReadDTO>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<List<CollectionWithCountDTO>> GetAllWithCountAsync(CancellationToken cancellationToken = default);
        Task<CollectionReadDTO> CreateAsync(CollectionCreateDTO collectionCreateDTO, CancellationToken cancellationToken = default);
        Task<CollectionReadDTO> UpdateAsync(Guid id, CollectionUpdateDTO collectionUpdateDTO, CancellationToken cancellationToken = default);
        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
