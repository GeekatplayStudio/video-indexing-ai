using System;
using System.IO;
using System.Text.Json;
using FootageSearch.Core.Interfaces;
using FootageSearch.Core.Models;

namespace FootageSearch.Core.Services
{
    public class JsonSettingsService : ISettingsService
    {
        private readonly string _filePath;

        public JsonSettingsService()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var appFolder = Path.Combine(appData, "FootageSearch");
            Directory.CreateDirectory(appFolder);
            _filePath = Path.Combine(appFolder, "settings.json");
        }

        public AppSettings LoadSettings()
        {
            if (!File.Exists(_filePath))
            {
                return new AppSettings();
            }

            try
            {
                var json = File.ReadAllText(_filePath);
                return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
            }
            catch
            {
                return new AppSettings();
            }
        }

        public void SaveSettings(AppSettings settings)
        {
            var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_filePath, json);
        }
    }
}
