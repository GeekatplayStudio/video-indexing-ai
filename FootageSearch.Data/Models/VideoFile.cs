using System;
using System.ComponentModel.DataAnnotations;

namespace FootageSearch.Data.Models
{
    public class VideoFile
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string FilePath { get; set; } = string.Empty;
        
        public string FileName { get; set; } = string.Empty;
        public double DurationSeconds { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string Format { get; set; } = string.Empty;
        
        public string Transcript { get; set; } = string.Empty;
        public string OcrText { get; set; } = string.Empty;
        public string VisualDescription { get; set; } = string.Empty;
        
        public DateTime IndexedAt { get; set; } = DateTime.Now;
        
        // For Qdrant correlation
        public Guid VectorId { get; set; } = Guid.NewGuid();
    }
}
