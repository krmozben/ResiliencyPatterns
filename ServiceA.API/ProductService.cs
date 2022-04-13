using ServiceA.API.Models;

namespace ServiceA.API
{
    public class ProductService
    {
        private readonly HttpClient _httpClient;

        public ProductService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<Product> GetProduct(int id)
        {
            var product = await _httpClient.GetFromJsonAsync<Product>($"{id}");
            return product;
        }
    }
}
