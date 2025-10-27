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
        private ClothyCatalogDbContext context;

        public UnitOfWork(ClothyCatalogDbContext context)
        {
            this.context = context;

            Brands = new BrandRepository(context);
            ClotheItems = new ClotheItemRepository(context);
            ClothesStocks = new ClothesStockRepository(context);
            Collections = new CollectionRepository(context);
            Colors = new ColorRepository(context);
            Materials = new MaterialRepository(context);
            Sizes = new SizeRepository(context);
            Tags = new TagRepository(context);
            ClothingTypes = new ClothingTypeRepository(context);
        }

        public IBrandRepository Brands { get; private set; }
        public IClotheItemRepository ClotheItems { get; private set; }
        public IClothesStockRepository ClothesStocks { get; private set; }
        public ICollectionRepository Collections { get; private set; }
        public IColorRepository Colors { get; private set; }
        public IMaterialRepository Materials { get; private set; }
        public ISizeRepository Sizes { get; private set; }
        public ITagRepository Tags { get; private set; }
        public IClothingTypeRepository ClothingTypes { get; private set; }

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
