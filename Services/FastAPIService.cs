using System.Text;
using System.Text.Json;
using VocaLens.DTOs.FastAPI;

namespace VocaLens.Services
{
    public class FastAPIService : IFastAPIService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public FastAPIService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _httpClient.BaseAddress = new Uri(_configuration["FastAPI:BaseUrl"] ?? throw new ArgumentNullException("FastAPI:BaseUrl"));
        }

        public async Task<FastAPIResponse<T>> GetAsync<T>(string endpoint)
        {
            var response = await _httpClient.GetAsync(endpoint);
            return await DeserializeResponseAsync<T>(response);
        }

        public async Task<FastAPIResponse<T>> PostAsync<T>(string endpoint, object data)
        {
            var content = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(endpoint, content);
            return await DeserializeResponseAsync<T>(response);
        }

        public async Task<FastAPIResponse<T>> PutAsync<T>(string endpoint, object data)
        {
            var content = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync(endpoint, content);
            return await DeserializeResponseAsync<T>(response);
        }

        public async Task<FastAPIResponse<T>> DeleteAsync<T>(string endpoint)
        {
            var response = await _httpClient.DeleteAsync(endpoint);
            return await DeserializeResponseAsync<T>(response);
        }

        private async Task<FastAPIResponse<T>> DeserializeResponseAsync<T>(HttpResponseMessage response)
        {
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            return JsonSerializer.Deserialize<FastAPIResponse<T>>(content, options) 
                ?? throw new JsonException("Failed to deserialize response");
        }
    }
}
