using Clothy.CatalogService.BLL.Interfaces;
using Clothy.CatalogService.DAL.UOW;
using Clothy.CatalogService.Domain.Entities;
using Clothy.Shared.Events.EmailEvents.ClotheStockUpdated;
using Clothy.Shared.Helpers.Exceptions;
using Clothy.Shared.Helpers.JWT;
using DnsClient.Internal;
using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.CatalogService.BLL.Services
{
    public class StockNotificationService : IStockNotificationService
    {
        private IUnitOfWork unitOfWork;
        private IUserClaimsExtractor userClaimsExtractor;
        private IPublishEndpoint publishEndpoint;
        private ILogger<StockNotificationService> logger;

        public StockNotificationService(IUnitOfWork unitOfWork, 
            IUserClaimsExtractor userClaimsExtractor,
            IPublishEndpoint publishEndpoint,
            ILogger<StockNotificationService> logger)
        {
            this.unitOfWork = unitOfWork;
            this.userClaimsExtractor = userClaimsExtractor;
            this.publishEndpoint = publishEndpoint;
            this.logger = logger;
        }

        public async Task NotifySubscribersAsync(Guid stockId, CancellationToken cancellationToken = default)
        {
            List<StockNotification> stockNotifications = await unitOfWork.StockNotification.GetAllSubscribersByStockId(stockId, cancellationToken);

            if (stockNotifications.Count > 0)
            {
                logger.LogInformation("Notifying {Count} subscribers for stock {StockId}", stockNotifications.Count, stockId);

                foreach (StockNotification stockNotification in stockNotifications)
                {
                    ClotheStockUpdatedEvent clotheStockUpdatedEvent = new ClotheStockUpdatedEvent
                    {
                        UserEmail = stockNotification.UserEmail,
                        UserFirstName = stockNotification.UserFirstName,
                        ClotheId = stockNotification.Stock!.ClotheId,
                        ClotheName = stockNotification.Stock!.Clothe!.Name,
                        Size = stockNotification.Stock!.Size!.Name,
                        Color = stockNotification.Stock!.Color!.HexCode,
                    };
                    await publishEndpoint.Publish(clotheStockUpdatedEvent, cancellationToken);

                    stockNotification.IsNotified = true;
                    unitOfWork.StockNotification.Update(stockNotification);
                }

                await unitOfWork.SaveChangesAsync();

                logger.LogInformation("Successfully notified {Count} subscribers for stock {StockId}", stockNotifications.Count, stockId);
            }
            else return;
        }

        public async Task SubscribeForStockAsync(Guid stockId, ClaimsPrincipal claimsPrincipal, CancellationToken cancellationToken = default)
        {
            ClothesStock? clothesStock = await unitOfWork.ClothesStocks.GetByIdAsync(stockId, cancellationToken);
            if (clothesStock == null) throw new NotFoundException($"Clothes stock not found with ID: {stockId}!");

            if (clothesStock.Quantity > 0) throw new ValidationFailedException("You cannot subscribe to clothing that is in stock!");

            Guid userId = userClaimsExtractor.GetUserId(claimsPrincipal);
            bool alreadySubscribed = await unitOfWork.StockNotification.HasUserAlreadySubscribeInStockId(userId, cancellationToken);
            if (alreadySubscribed) throw new AlreadyExistsException("You are already subscribed to this stock!");

            StockNotification stockNotification = new StockNotification
            {
                StockId = stockId,
                UserId = userId,
                UserEmail = userClaimsExtractor.GetEmail(claimsPrincipal),
                UserFirstName = userClaimsExtractor.GetFirstName(claimsPrincipal),
                IsNotified = false,
            };
            await unitOfWork.StockNotification.AddAsync(stockNotification, cancellationToken);
            await unitOfWork.SaveChangesAsync();
        }
    }
}
