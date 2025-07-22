using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using VocaLens.Data;
using VocaLens.DTOs;
using VocaLens.Models;

public class ChatService
{
    private readonly AudioDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private static string _currentChoice = "arabic"; // Default choice

    public ChatService(AudioDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public void SetCurrentChoice(string choice)
    {
        _currentChoice = choice?.ToLower() ?? "arabic";
    }

    public string GetCurrentChoice()
    {
        return _currentChoice;
    }

    public async Task<List<ChatHistoryDto>> GetChatHistoryAsync(string filterType = null)
    {
        var userId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            return new(); 

        var filter = filterType?.ToLower() ?? _currentChoice;

        var query = _context.AudioRecordings
            .Where(c => c.UserId == userId);

        switch (filter)
        {
            case "arabic":
                query = query.Where(c => c.TranscribedTextArabic != null);
                break;
            case "translation":
                query = query.Where(c => c.TranscribedTranslated != null);
                break;
            case "english":
                query = query.Where(c => c.TranscribedTextEnglish != null);
                break;
        }

        return await query
            .OrderBy(c => c.Id)
            .Select(c => new ChatHistoryDto
            {
                Id = c.Id,
                EnglishText = c.TranscribedTextEnglish,
                ArabicText = c.TranscribedTextArabic,
                TranslatedText = c.TranscribedTranslated,
                Timestamp = c.RecordedAt
            })
            .ToListAsync();
    }
}
