using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using VocaLens.Models;
using VocaLens.Service;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using VocaLens.Data;
using VocaLens.Hubs;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;


namespace VocaLens.Controllers
{
    [Authorize]
    [Route("api/audio")]
    [ApiController]
    public class AudioController : ControllerBase
    {
        private readonly AudioDbContext _context;
        private readonly AzureSpeechService _azureSpeechService;
        private readonly IHubContext<AudioHub> _hubContext;
        private readonly IHttpClientFactory _httpClientFactory;


        public AudioController(AudioDbContext context, AzureSpeechService azureSpeechService, IHubContext<AudioHub> hubContext, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _azureSpeechService = azureSpeechService;
            _hubContext = hubContext;
            _httpClientFactory = httpClientFactory;
        }

        // Endpoint to transcribe English audio
        [HttpPost("transcribe/en")]
        public async Task<IActionResult> TranscribeEnglishAudio(IFormFile audioFile)
        {
            if (audioFile == null || audioFile.Length == 0)
            {
                return BadRequest("No audio file was uploaded.");
            }

            try
            {
                var tempFilePath = Path.GetTempFileName();
                using (var stream = new FileStream(tempFilePath, FileMode.Create))
                {
                    await audioFile.CopyToAsync(stream);
                }

                // Transcribe English speech
                var transcriptionEnglish = await _azureSpeechService.ConvertSpeechToText(tempFilePath, "en-US");

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                // Save to the database
                byte[] audioData;
                using (var memoryStream = new MemoryStream())
                {
                    await audioFile.CopyToAsync(memoryStream);
                    audioData = memoryStream.ToArray();
                }

                var audioRecording = new AudioRecording
                {
                    AudioData = audioData,
                    TranscribedTextEnglish = transcriptionEnglish,
                    UserId = userId
                };

                _context.AudioRecordings.Add(audioRecording);
                await _context.SaveChangesAsync();

                return Ok(transcriptionEnglish);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error transcribing audio: {ex.Message} | Inner: {ex.InnerException?.Message}");
            }
        }

        // Endpoint to transcribe Arabic audio
        [HttpPost("transcribe/ar")]
        public async Task<IActionResult> TranscribeArabicAudio(IFormFile audioFile)
        {
            if (audioFile == null || audioFile.Length == 0)
            {
                return BadRequest("No audio file was uploaded.");
            }

            try
            {
                var tempFilePath = Path.GetTempFileName();
                using (var stream = new FileStream(tempFilePath, FileMode.Create))
                {
                    await audioFile.CopyToAsync(stream);
                }

                // Transcribe Arabic speech
                var transcriptionArabic = await _azureSpeechService.ConvertSpeechToText(tempFilePath, "ar-SA");

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                // Save to the database
                byte[] audioData;
                using (var memoryStream = new MemoryStream())
                {
                    await audioFile.CopyToAsync(memoryStream);
                    audioData = memoryStream.ToArray();
                }

                var audioRecording = new AudioRecording
                {
                    AudioData = audioData,
                    TranscribedTextArabic = transcriptionArabic,
                    UserId = userId
                };

                _context.AudioRecordings.Add(audioRecording);
                await _context.SaveChangesAsync();

                return Ok(transcriptionArabic);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error transcribing audio: {ex.Message} | Inner: {ex.InnerException?.Message}");
            }
        }

        // Endpoint to transcribe English audio and provide Arabic translation
        [HttpPost("transcribe/en-to-ar")]
        public async Task<IActionResult> TranscribeEnglishAndArabicAudio(IFormFile audioFile)
        {
            if (audioFile == null || audioFile.Length == 0)
            {
                return BadRequest("No audio file was uploaded.");
            }

            try
            {
                var tempFilePath = Path.GetTempFileName();
                using (var stream = new FileStream(tempFilePath, FileMode.Create))
                {
                    await audioFile.CopyToAsync(stream);
                }

                // Transcribe English speech
                var transcriptionEnglish = await _azureSpeechService.ConvertSpeechToText(tempFilePath, "en-US");

                // Translate English text to Arabic
                var transcriptionArabic = await _azureSpeechService.TranslateTextAsync(transcriptionEnglish, "ar");

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                // Save to the database
                byte[] audioData;
                using (var memoryStream = new MemoryStream())
                {
                    await audioFile.CopyToAsync(memoryStream);
                    audioData = memoryStream.ToArray();
                }

                var audioRecording = new AudioRecording
                {
                    AudioData = audioData,
                    //TranscribedTextEnglish = transcriptionEnglish,
                    TranscribedTranslated = transcriptionArabic,
                    UserId = userId
                };

                _context.AudioRecordings.Add(audioRecording);
                await _context.SaveChangesAsync();

                return Ok(transcriptionArabic);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error transcribing audio: {ex.Message} | Inner: {ex.InnerException?.Message}");
            }
        }

        [HttpPost("transcribe-custom/ar")]
        public async Task<IActionResult> CustomTranscribeArabic(IFormFile audioFile)
        {
            if (audioFile == null || audioFile.Length == 0)
                return BadRequest("No file uploaded.");

            using var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromMinutes(2);
            using var content = new MultipartFormDataContent();
            await using var stream = audioFile.OpenReadStream();
            var fileContent = new StreamContent(stream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("audio/wav");

            // Important: field name must match FastAPI — "file"
            content.Add(fileContent, "file", audioFile.FileName);

            // 🔥 Use HTTPS for ngrok
            var transcribeResponse = await client.PostAsync("https://9b4c-102-186-19-254.ngrok-free.app/transcribe-openai", content);

            if (!transcribeResponse.IsSuccessStatusCode)
            {
                var error = await transcribeResponse.Content.ReadAsStringAsync();
                return StatusCode((int)transcribeResponse.StatusCode, $"Failed to transcribe: {error}");
            }

            var responseContent = await transcribeResponse.Content.ReadAsStringAsync();
            var json = JObject.Parse(responseContent);

            if (json.ContainsKey("error"))
                return BadRequest(json["error"]?.ToString());

            var transcription = json["transcription"]?.ToString();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            byte[] audioData;
            using (var memoryStream = new MemoryStream())
            {
                await audioFile.CopyToAsync(memoryStream);
                audioData = memoryStream.ToArray();
            }

            var audioRecording = new AudioRecording
            {
                AudioData = audioData,
                //TranscribedTextEnglish = transcriptionEnglish,
                TranscribedTextArabic = transcription,
                UserId = userId
            };

            _context.AudioRecordings.Add(audioRecording);
            await _context.SaveChangesAsync();

            return Ok( json["transcription"]?.ToString());
        }
        [HttpPost("transcribe-custom/en")]
        public async Task<IActionResult> CustomTranscribeEnglish(IFormFile audioFile)
        {
            if (audioFile == null || audioFile.Length == 0)
                return BadRequest("No file uploaded.");

            using var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromMinutes(2);
            using var content = new MultipartFormDataContent();
            await using var stream = audioFile.OpenReadStream();
            var fileContent = new StreamContent(stream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("audio/wav");

            // Important: field name must match FastAPI — "file"
            content.Add(fileContent, "file", audioFile.FileName);

            // 🔥 Use HTTPS for ngrok
            var transcribeResponse = await client.PostAsync("https://9b4c-102-186-19-254.ngrok-free.app/transcribe-distil", content);

            if (!transcribeResponse.IsSuccessStatusCode)
            {
                var error = await transcribeResponse.Content.ReadAsStringAsync();
                return StatusCode((int)transcribeResponse.StatusCode, $"Failed to transcribe: {error}");
            }

            var responseContent = await transcribeResponse.Content.ReadAsStringAsync();
            var json = JObject.Parse(responseContent);

            if (json.ContainsKey("error"))
                return BadRequest(json["error"]?.ToString());

            var transcription = json["transcription"]?.ToString();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            byte[] audioData;
            using (var memoryStream = new MemoryStream())
            {
                await audioFile.CopyToAsync(memoryStream);
                audioData = memoryStream.ToArray();
            }

            var audioRecording = new AudioRecording
            {
                AudioData = audioData,
                //TranscribedTextEnglish = transcriptionEnglish,
                TranscribedTextEnglish = transcription,
                UserId = userId
            };

            _context.AudioRecordings.Add(audioRecording);
            await _context.SaveChangesAsync();

            return Ok(json["transcription"]?.ToString());
        }
    }
}
