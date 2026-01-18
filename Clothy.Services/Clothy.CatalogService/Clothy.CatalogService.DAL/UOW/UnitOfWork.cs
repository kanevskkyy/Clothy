using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.CatalogService.DAL.DB;
using Clothy.CatalogService.DAL.Interfaces;
using Clothy.CatalogService.DAL.Repositories;
using Clothy.CatalogService.Domain.Entities;
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
        public IStockNotificationRepository StockNotification { get; }
        public IClothePopularityRepository ClothePopularity { get; }

        private ClothyCatalogDbContext context;

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
            IClothingTypeRepository clothingTypes,
            IStockNotificationRepository stockNotification,
            IClothePopularityRepository clothePopularity)
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
            StockNotification = stockNotification;
            ClothePopularity = clothePopularity;
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await context.SaveChangesAsync(cancellationToken);
        }

        public void Dispose()
        {
            context.Dispose();
        }
    }
}
