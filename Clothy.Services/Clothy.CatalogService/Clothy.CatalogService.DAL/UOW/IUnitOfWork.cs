using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.CatalogService.DAL.Interfaces;

namespace Clothy.CatalogService.DAL.UOW
{
    public interface IUnitOfWork : IDisposable
    {
        IBrandRepository Brands { get; }
        IClotheItemRepository ClotheItems { get; }
        IClothesStockRepository ClothesStocks { get; }
        ICollectionRepository Collections { get; }
        IColorRepository Colors { get; }
        IMaterialRepository Materials { get; }
        ISizeRepository Sizes { get; }
        ITagRepository Tags { get; }
        IClothingTypeRepository ClothingTypes { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
