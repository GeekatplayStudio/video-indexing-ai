namespace FootageSearch.Core.Models
{
    public class JobStatus
    {
        public bool IsRunning { get; set; }
        public string CurrentTask { get; set; } = string.Empty;
        public string Detail { get; set; } = string.Empty;
        public double? ProgressPercentage { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
