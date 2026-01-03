using System.ComponentModel.DataAnnotations;

namespace FootageSearch.Data.Models
{
    public class JobStatusEntity
    {
        [Key]
        public int Id { get; set; }
        public string ServiceName { get; set; } = string.Empty; // "Indexer", "Api", etc.
        public bool IsRunning { get; set; }
        public string CurrentTask { get; set; } = string.Empty;
        public string Detail { get; set; } = string.Empty;
        public double? ProgressPercentage { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
