using SQLite;

namespace SimpleImageOCR.Models
{
    [Table("OcrResults")]
    public class OcrResult
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [NotNull]
        public string ExtractedText { get; set; } = string.Empty;

        [NotNull]
        public string ImagePath { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public string Title { get; set; } = string.Empty;

        public bool IsFavorite { get; set; } = false;

        public string Tags { get; set; } = string.Empty;

        // For display purposes
        public string PreviewText => ExtractedText.Length > 100 
            ? ExtractedText.Substring(0, 100) + "..." 
            : ExtractedText;

        public string FormattedDate => CreatedAt.ToString("MMM dd, yyyy HH:mm");
    }
}