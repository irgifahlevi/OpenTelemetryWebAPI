using Order.API.DTO;
using Order.API.Extension;
using Order.API.Model;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace Order.API.Services
{
    public class ProductService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;

        public ProductService(HttpClient httpClient, ILogger<ErrorHandlingMiddleware> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }
        public async Task<OrderProduct?> GetSyncProduct(int productId)
        {
            var response = new OrderProduct();
            try
            {
                _httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                string url = $"http://localhost:5038/api/Product/{productId}";

                using (var requestMessage = new HttpRequestMessage(HttpMethod.Get, url))
                {
                    var httpResponse = await _httpClient.SendAsync(requestMessage);

                    if (httpResponse.IsSuccessStatusCode)
                    {
                        var json = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                        var responseData = JsonSerializer.Deserialize<ProductSyncResponse>(json, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });
                        if (responseData != null)
                        {
                            response.Id = responseData?.Data?.Id ?? 0;
                            response.Name = responseData?.Data?.Name ?? "";
                            response.Price = responseData?.Data?.Price ?? 0;
                            return response;
                        }
                        
                    }
                    else
                    {
                        _logger.LogWarning(httpResponse.ReasonPhrase, "Business rule violated");
                    }
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }

            return null;

        }
    }
}
