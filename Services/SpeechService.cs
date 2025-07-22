using VocaLens.Services.Interfaces;

namespace VocaLens.Services
{
    public class SpeechService : ISpeechService
    {
        public Task<string> TranscribeAsync(byte[] audioData)
        {
            return Task.FromResult("This is a transcribed result.");
        }
    }
}