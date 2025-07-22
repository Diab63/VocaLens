using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace VocaLens.Service
{
    public class AzureSpeechService
    {
        private readonly string subscriptionKey = "key";
        private readonly string region = "region";

        // Method to recognize speech from an audio file (either English or Arabic)
        public async Task<string> ConvertSpeechToText(string audioFilePath, string language)
        {
            try
            {
                var speechConfig = SpeechConfig.FromSubscription(subscriptionKey, region);
                speechConfig.SpeechRecognitionLanguage = language; // Set language dynamically (English or Arabic)

                using var audioConfig = AudioConfig.FromWavFileInput(audioFilePath);
                using var speechRecognizer = new SpeechRecognizer(speechConfig, audioConfig);

                var result = await speechRecognizer.RecognizeOnceAsync();

                return result.Reason == ResultReason.RecognizedSpeech
                    ? result.Text
                    : $"Speech recognition failed: {result.Reason}";
            }
            catch (Exception ex)
            {
                return $"Error in speech recognition: {ex.Message}";
            }
        }

        // Method to translate text (e.g., from English to Arabic)
        public async Task<string> TranslateTextAsync(string text, string targetLanguage)
        {
            try
            {
                var endpoint = $"https://api.cognitive.microsofttranslator.com/translate?api-version=3.0&to={targetLanguage}";

                using var client = new HttpClient();
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "key");
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Region", "region");

                var requestBody = new[] { new { Text = text } };
                var content = new StringContent(System.Text.Json.JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

                var response = await client.PostAsync(endpoint, content);
                var responseBody = await response.Content.ReadAsStringAsync();

                using (JsonDocument doc = JsonDocument.Parse(responseBody))
                {
                    var root = doc.RootElement;
                    if (root.ValueKind == JsonValueKind.Array && root.GetArrayLength() > 0)
                    {
                        var translations = root[0].GetProperty("translations");
                        if (translations.ValueKind == JsonValueKind.Array && translations.GetArrayLength() > 0)
                        {
                            return translations[0].GetProperty("text").GetString();
                        }
                    }
                }

                return "Translation failed: No translation found in response.";
            }
            catch (Exception ex)
            {
                return $"Translation failed: {ex.Message}";
            }
        }
    }
}
