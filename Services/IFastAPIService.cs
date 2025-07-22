using VocaLens.DTOs.FastAPI;

namespace VocaLens.Services
{
    public interface IFastAPIService
    {
        Task<FastAPIResponse<T>> GetAsync<T>(string endpoint);
        Task<FastAPIResponse<T>> PostAsync<T>(string endpoint, object data);
        Task<FastAPIResponse<T>> PutAsync<T>(string endpoint, object data);
        Task<FastAPIResponse<T>> DeleteAsync<T>(string endpoint);
    }
}
