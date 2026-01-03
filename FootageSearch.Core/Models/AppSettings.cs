using System.Collections.Generic;

namespace FootageSearch.Core.Models
{
    public class AppSettings
    {
        public List<string> WatchFolders { get; set; } = new List<string>();
        public string TempFolderPath { get; set; } = "Temp";
        
        // AI Configuration
        public string OllamaApiUrl { get; set; } = "http://localhost:11434";
        public string OllamaModel { get; set; } = "llama3.2-vision";
        public string OllamaEmbeddingModel { get; set; } = "nomic-embed-text";
        public string WhisperModelType { get; set; } = "Base"; // Tiny, Base, Small, Medium, Large
    }
}
