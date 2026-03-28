using Clothy.OrderService.BLL.Config;
using Clothy.OrderService.BLL.DTOs.APIClientDTOs;
using Clothy.OrderService.BLL.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace Clothy.OrderService.BLL.Services
{
    public class NovaPoshtaAPIClient : IDeliveryAPIClient
    {
        private ILogger<NovaPoshtaAPIClient> logger;
        private HttpClient httpClient;
        private NovaPoshtaConfig poshtaConfig;

        private const string MODEL_NAME = "Address";
        private const string AREAS_CALLED_METHOD = "getAreas";
        private const string CITIES_CALLED_METHOD = "getCities";
        private const string WAREHOUSES_CALLED_METHOD = "getWarehouses";
        private const string BASE_URL = "https://api.novaposhta.ua/v2.0/json/";

        public NovaPoshtaAPIClient(
            ILogger<NovaPoshtaAPIClient> logger,
            IHttpClientFactory httpClientFactory,
            IOptions<NovaPoshtaConfig> options)
        {
            this.logger = logger;
            httpClient = httpClientFactory.CreateClient("NovaPoshtaAPI");
            poshtaConfig = options.Value;
        }

        public async Task<List<RegionDTO>> GetAreasAsync(CancellationToken cancellationToken = default)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            logger.LogInformation("Fetching regions from NovaPoshta API");

            object body = new
            {
                apiKey = poshtaConfig.APIKey,
                modelName = MODEL_NAME,
                calledMethod = AREAS_CALLED_METHOD
            };
            string json = JsonSerializer.Serialize(body);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage httpResponseMessage = await httpClient.PostAsync(BASE_URL, content, cancellationToken);
            string result = await httpResponseMessage.Content.ReadAsStringAsync(cancellationToken);

            NovaPoshtaResponse<RegionDTO>? novaPoshtaResponse = JsonSerializer.Deserialize<NovaPoshtaResponse<RegionDTO>>(result,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }
            );

            stopwatch.Stop();
            logger.LogInformation("Fetched {Count} regions in {ElapsedMs}ms", novaPoshtaResponse?.Data?.Count ?? 0, stopwatch.ElapsedMilliseconds);

            return novaPoshtaResponse?.Data!;
        }

        public async Task<List<SettlementDTO>> GetSettlementsByAreaRefAsync(string areaRef, CancellationToken cancellationToken = default)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            logger.LogDebug("Fetching settlements for area {AreaRef}", areaRef);

            object body = new
            {
                apiKey = poshtaConfig.APIKey,
                modelName = MODEL_NAME,
                calledMethod = CITIES_CALLED_METHOD,
                methodProperties = new
                {
                    AreaRef = areaRef,
                }
            };
            string json = JsonSerializer.Serialize(body);
            StringContent stringContent = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage httpResponseMessage = await httpClient.PostAsync(BASE_URL, stringContent, cancellationToken);
            string result = await httpResponseMessage.Content.ReadAsStringAsync(cancellationToken);

            NovaPoshtaResponse<SettlementDTO>? citiesResponse = JsonSerializer.Deserialize<NovaPoshtaResponse<SettlementDTO>>(
               result,
               new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            stopwatch.Stop();
            logger.LogDebug("Fetched {Count} settlements for area {AreaRef} in {ElapsedMs}ms", citiesResponse?.Data?.Count ?? 0, areaRef, stopwatch.ElapsedMilliseconds);

            return citiesResponse?.Data!;
        }

        public async Task<List<PickupPointDTO>> GetWarehousesByCityRefAsync(string cityRef, CancellationToken cancellationToken = default)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            object body = new
            {
                apiKey = poshtaConfig.APIKey,
                modelName = MODEL_NAME,
                calledMethod = WAREHOUSES_CALLED_METHOD,
                methodProperties = new
                {
                    CityRef = cityRef,
                }
            };
            string json = JsonSerializer.Serialize(body);
            StringContent stringContent = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage httpResponseMessage = await httpClient.PostAsync(BASE_URL, stringContent, cancellationToken);
            string result = await httpResponseMessage.Content.ReadAsStringAsync(cancellationToken);

            NovaPoshtaResponse<PickupPointDTO>? novaPoshtaResponse = JsonSerializer.Deserialize<NovaPoshtaResponse<PickupPointDTO>>(
                result,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            stopwatch.Stop();
            if (novaPoshtaResponse?.Data?.Count > 0) logger.LogTrace("Fetched {Count} warehouses for city {CityRef} in {ElapsedMs}ms", novaPoshtaResponse.Data.Count, cityRef, stopwatch.ElapsedMilliseconds);

            return novaPoshtaResponse?.Data!;
        }
    }
}