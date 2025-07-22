namespace VocaLens.Models
{
    public class ChatHistoryDto
    {
        public int Id { get; set; }
        public string EnglishText { get; set; }
        public string ArabicText { get; set; }
        public string? TranslatedText { get; set; } // ✅ optional

        public DateTime Timestamp { get; set; }
    }
}
