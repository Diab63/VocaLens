namespace VocaLens.Services.Interfaces
{
    public interface ISpeechService
    {
        Task<string> TranscribeAsync(byte[] audioData);
    }
}