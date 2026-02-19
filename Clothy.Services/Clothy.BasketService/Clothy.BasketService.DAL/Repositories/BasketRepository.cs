using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Clothy.BasketService.DAL.Repositories.Interfaces;
using Clothy.BaskteService.Domain.Entities;
using StackExchange.Redis;

namespace Clothy.BasketService.DAL.Repositories
{
    public class BasketRepository : IBasketRepository
    {
        private IDatabase database;

        public BasketRepository(IConnectionMultiplexer connectionMultiplexer)
        {
            database = connectionMultiplexer.GetDatabase();
        }

        public async Task UpdateItemQuantityAsync(Guid userId, Guid clotheId, Guid sizeId, Guid colorId, int quantity)
        {
            BasketList? basket = await GetBasketAsync(userId);
            if (basket == null) return;

            BasketItem? item = basket.BasketItems.FirstOrDefault(
                basketItem => basketItem.ClotheId == clotheId
                           && basketItem.SizeId == sizeId
                           && basketItem.ColorId == colorId
            );

            if (item == null) return;

            item.Quantity = quantity;

            string serialized = JsonSerializer.Serialize(basket);
            await database.StringSetAsync(userId.ToString(), serialized);
        }   

        public async Task AddOrUpdateItemAsync(Guid userId, BasketItem item)
        {
            BasketList? basket = await GetBasketAsync(userId);
            if (basket == null) basket = new BasketList
            {
                UserId = userId
            };      

            BasketItem? basketItem = basket.BasketItems.FirstOrDefault(p => p.ClotheId == item.ClotheId && p.SizeId == item.SizeId && p.ColorId == item.ColorId);
            if (basketItem != null) basketItem.Quantity += item.Quantity;
            else basket.BasketItems.Add(item);

            string serialized = JsonSerializer.Serialize(basket);
            await database.StringSetAsync(userId.ToString(), serialized);
        }

        public async Task ClearBasketAsync(Guid userId)
        {
            await database.KeyDeleteAsync(userId.ToString());
        }

        public async Task<BasketList?> GetBasketAsync(Guid userId)
        {
            RedisValue data = await database.StringGetAsync(userId.ToString());
            if (data.IsNullOrEmpty) return null;
            
            return JsonSerializer.Deserialize<BasketList>(data);   
        }

        public async Task RemoveItemAsync(Guid userId, Guid clotheId, Guid sizeId, Guid colorId)
        {
            BasketList? basketList = await GetBasketAsync(userId);
            if (basketList == null) return;

            basketList.BasketItems.RemoveAll(p => p.ColorId == colorId && p.SizeId == sizeId && p.ClotheId == clotheId);

            string serialized = JsonSerializer.Serialize(basketList);
            await database.StringSetAsync(userId.ToString(), serialized);
        }
    }
}
