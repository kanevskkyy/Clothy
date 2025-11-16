using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.OrderService.BLL.Interfaces;
using Clothy.OrderService.DAL.UOW;
using Clothy.OrderService.Domain.Entities;
using Clothy.Shared.Cache.Interfaces;
using Clothy.Shared.Events.ClotheItem;

namespace Clothy.OrderService.BLL.Services
{
    public class OrderItemService : IOrderItemService
    {
        private IUnitOfWork unitOfWork;
        private IEntityCacheInvalidationService<Order> cacheInvalidationService;

        public OrderItemService(IUnitOfWork unitOfWork, IEntityCacheInvalidationService<Order> cacheInvalidationService)
        {
            this.unitOfWork = unitOfWork;
            this.cacheInvalidationService = cacheInvalidationService;
        }

        public async Task UpdateOrderItemsAsync(ClotheItemUpdatedEvent clotheItemUpdatedEvent, CancellationToken cancellationToken = default)
        {
            List<OrderItem> orderItems = await unitOfWork.OrderItems.GetByClotheIdAsync(clotheItemUpdatedEvent.ClotheId, cancellationToken);

            if (orderItems.Count == 0) return;

            foreach(OrderItem item in orderItems)
            {
                item.ClotheName = clotheItemUpdatedEvent.ClotheName;
                item.Price = clotheItemUpdatedEvent.Price;
                item.MainPhoto = clotheItemUpdatedEvent.MainPhoto;

                await unitOfWork.OrderItems.UpdateAsync(item, cancellationToken);
            }
            await unitOfWork.CommitAsync();
            await cacheInvalidationService.InvalidateAllAsync();
        }
    }
}
