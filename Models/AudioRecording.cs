using VocaLens.Entities;
using System;
using System.ComponentModel.DataAnnotations;

namespace VocaLens.Models
{
    public class AudioRecording
    {
        [Key]
        public int Id { get; set; }
        public byte[]? AudioData { get; set; }
        public string? TranscribedTextEnglish { get; set; }
        public string? TranscribedTextArabic { get; set; }

        public string? TranscribedTranslated { get; set; }
        public DateTime RecordedAt { get; set; } = DateTime.UtcNow;

        public string UserId { get; set; } = string.Empty;
        

    }
}
