using System.Text.Json.Serialization;

namespace VocaLens.DTOs.FastAPI
{
    public class FastAPIResponse<T>
    {
        [JsonPropertyName("data")]
        public T? Data { get; set; }

        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }
    }
}
