using Clothy.CatalogService.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clothy.CatalogService.API.Controllers
{
    [ApiController]
    [Route("api/stocks/notifications")]
    public class StockNotificationController : ControllerBase
    {
        private IStockNotificationService stockNotificationService;
        private ILogger<StockNotificationController> logger;

        public StockNotificationController(
            IStockNotificationService stockNotificationService,
            ILogger<StockNotificationController> logger)
        {
            this.stockNotificationService = stockNotificationService;
            this.logger = logger;
        }

        /// <summary>
        /// Subscribe to a stock notification for a specific clothing item.
        /// </summary>
        /// <param name="stockId">ID of the stock item.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        [HttpPost("subscribe/{stockId:guid}")]
        [Authorize] 
        public async Task<ActionResult> Subscribe(Guid stockId, CancellationToken cancellationToken)
        {
            logger.LogInformation("User {User} is subscribing to stock {StockId}.", User.Identity?.Name, stockId);
            await stockNotificationService.SubscribeForStockAsync(stockId, User, cancellationToken);
            
            logger.LogInformation("User {User} successfully subscribed to stock {StockId}.", User.Identity?.Name, stockId);
            return NoContent();
        }
    }
}
