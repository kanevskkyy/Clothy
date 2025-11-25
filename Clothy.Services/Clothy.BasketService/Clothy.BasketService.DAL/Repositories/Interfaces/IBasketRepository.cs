using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.BaskteService.Domain.Entities;

namespace Clothy.BasketService.DAL.Repositories.Interfaces
{
    public interface IBasketRepository
    {
        Task<BasketList?> GetBasketAsync(Guid userId);
        Task AddOrUpdateItemAsync(Guid userId, BasketItem item);
        Task RemoveItemAsync(Guid userId, Guid clotheId, Guid sizeId, Guid colorId);
        Task UpdateItemQuantityAsync(Guid userId, Guid clotheId, Guid sizeId, Guid colorId, int quantity);
        Task ClearBasketAsync(Guid userId);
    }
}
