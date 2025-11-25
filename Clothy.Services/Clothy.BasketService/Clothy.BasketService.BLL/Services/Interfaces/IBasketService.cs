using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.BasketService.BLL.DTOs;

namespace Clothy.BasketService.BLL.Services.Interfaces
{
    public interface IBasketService
    {
        Task<BasketDTO?> GetBasketAsync(Guid userId);
        Task<BasketDTO> AddOrUpdateItemAsync(Guid userId, BasketItemCreateDTO itemDto);
        Task<BasketDTO> UpdateItemQuantityAsync(Guid userId, BasketItemUpdateDTO updateDto);
        Task RemoveItemAsync(Guid userId, Guid clotheId, Guid sizeId, Guid colorId);
        Task ClearBasketAsync(Guid userId);
    }
}
