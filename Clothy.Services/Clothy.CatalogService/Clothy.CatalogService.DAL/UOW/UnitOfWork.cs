using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.CatalogService.DAL.DB;
using Clothy.CatalogService.DAL.Interfaces;
using Clothy.CatalogService.DAL.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Clothy.CatalogService.DAL.UOW
{
    public class UnitOfWork : IUnitOfWork
    {
        public IBrandRepository Brands { get; }
        public IClotheItemRepository ClotheItems { get; }
        public IClothesStockRepository ClothesStocks { get; }
        public ICollectionRepository Collections { get; }
        public IColorRepository Colors { get; }
        public IMaterialRepository Materials { get; }
        public ISizeRepository Sizes { get; }
        public ITagRepository Tags { get; }
        public IClothingTypeRepository ClothingTypes { get; }

        private readonly ClothyCatalogDbContext context;

        public UnitOfWork(
            ClothyCatalogDbContext context,
            IBrandRepository brands,
            IClotheItemRepository clotheItems,
            IClothesStockRepository clothesStocks,
            ICollectionRepository collections,
            IColorRepository colors,
            IMaterialRepository materials,
            ISizeRepository sizes,
            ITagRepository tags,
            IClothingTypeRepository clothingTypes)
        {
            this.context = context;

            Brands = brands;
            ClotheItems = clotheItems;
            ClothesStocks = clothesStocks;
            Collections = collections;
            Colors = colors;
            Materials = materials;
            Sizes = sizes;
            Tags = tags;
            ClothingTypes = clothingTypes;
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var strategy = context.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
                try
                {
                    var result = await context.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);
                    return result;
                }
                catch
                {
                    await transaction.RollbackAsync(cancellationToken);
                    throw;
                }
            });
        }

        public void Dispose()
        {
            context.Dispose();
        }
    }
}
