using Clothy.BasketService.BLL.DTOs;
using Clothy.BasketService.BLL.Services.Interfaces;
using Clothy.Shared.Helpers.JWT;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clothy.BasketService.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/basket")]
    public class BasketController : ControllerBase
    {
        private IBasketService basketService;
        private IUserClaimsExtractor userClaimsExtractor;
        private ILogger<BasketController> logger;

        public BasketController(IBasketService basketService, IUserClaimsExtractor userClaimsExtractor, ILogger<BasketController> logger)
        {
            this.basketService = basketService;
            this.userClaimsExtractor = userClaimsExtractor;
            this.logger = logger;
        }

        /// <summary>
        /// Gets the basket for the authenticated user.
        /// </summary>
        /// <returns>The user's basket with items and total price.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(BasketDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetBasketAsync()
        {
            Guid userId = userClaimsExtractor.GetUserId(User);
            logger.LogInformation("Fetching basket for user {UserId}", userId);

            BasketDTO? basket = await basketService.GetBasketAsync(userId);
            return Ok(basket);
        }

        /// <summary>
        /// Adds a new item to the basket or updates it if it already exists.
        /// </summary>
        /// <param name="basketItemCreateDTO">Item data including size, color, price and quantity.</param>
        /// <returns>Updated basket with the new item.</returns>
        [HttpPost("items")]
        [ProducesResponseType(typeof(BasketDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddItemAsync([FromBody] BasketItemCreateDTO basketItemCreateDTO)
        {
            Guid userId = userClaimsExtractor.GetUserId(User);
            logger.LogInformation("Adding or updating basket item for user {UserId}. ClotheId: {ClotheId}, SizeId: {SizeId}, ColorId: {ColorId}", userId, basketItemCreateDTO.ClotheId, basketItemCreateDTO.SizeId, basketItemCreateDTO.ColorId);

            BasketDTO basket = await basketService.AddOrUpdateItemAsync(userId, basketItemCreateDTO);
            return Ok(basket);
        }

        /// <summary>
        /// Updates the quantity of a specific basket item.
        /// </summary>
        /// <param name="dto">Update data including clothe ID and new quantity.</param>
        /// <returns>Updated basket with modified quantity.</returns>
        [HttpPut("items")]
        [ProducesResponseType(typeof(BasketDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateItemQuantityAsync([FromBody] BasketItemUpdateDTO dto)
        {
            Guid userId = userClaimsExtractor.GetUserId(User);
            logger.LogInformation("Updating quantity for user {UserId}. ClotheId: {ClotheId}, Quantity: {Qty}", userId, dto.ClotheId, dto.Quantity);

            BasketDTO basket = await basketService.UpdateItemQuantityAsync(userId, dto);
            return Ok(basket);
        }

        /// <summary>
        /// Removes a specific item from the basket.
        /// </summary>
        /// <param name="clothItemId">Clothe ID.</param>
        /// <param name="sizeId">Size ID.</param>
        /// <param name="colorId">Color ID.</param>
        /// <returns>Success status.</returns>
        [HttpDelete("items/{clothItemId}/{sizeId}/{colorId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RemoveItemAsync(Guid clothItemId, Guid sizeId, Guid colorId)
        {
            Guid userId = userClaimsExtractor.GetUserId(User);
            logger.LogInformation("Removing basket item for user {UserId}. ClotheId: {ClotheId}, SizeId: {SizeId}, ColorId: {ColorId}", userId, clothItemId, sizeId, colorId);

            await basketService.RemoveItemAsync(userId, clothItemId, sizeId, colorId);
            return NoContent();
        }

        /// <summary>
        /// Clears the entire basket for the authenticated user.
        /// </summary>
        /// <returns>Success status.</returns>
        [HttpDelete]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> ClearBasketAsync()
        {
            Guid userId = userClaimsExtractor.GetUserId(User);
            logger.LogInformation("Clearing basket for user {UserId}", userId);

            await basketService.ClearBasketAsync(userId);
            return NoContent();
        }
    }
}