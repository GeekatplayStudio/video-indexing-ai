using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using FootageSearch.Core.Interfaces;
using FootageSearch.Core.Models;

namespace FootageSearch.Core.Services
{
    public class FileJobStatusService : IJobStatusService
    {
        private readonly string _statusFilePath;
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions { WriteIndented = true };

        public FileJobStatusService()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var folder = Path.Combine(appData, "FootageSearch");
            Directory.CreateDirectory(folder);
            _statusFilePath = Path.Combine(folder, "status.json");
        }

        public async Task UpdateStatusAsync(string task, string detail, double? progress = null)
        {
            var status = new JobStatus
            {
                IsRunning = true,
                CurrentTask = task,
                Detail = detail,
                ProgressPercentage = progress,
                LastUpdated = DateTime.Now
            };

            try
            {
                var json = JsonSerializer.Serialize(status, _jsonOptions);
                await File.WriteAllTextAsync(_statusFilePath, json);
            }
            catch
            {
                // Ignore concurrency errors for now
            }
        }

        public async Task<JobStatus> GetStatusAsync()
        {
            try
            {
                if (!File.Exists(_statusFilePath))
                {
                    return new JobStatus { IsRunning = false, CurrentTask = "Idle" };
                }

                var json = await File.ReadAllTextAsync(_statusFilePath);
                return JsonSerializer.Deserialize<JobStatus>(json) ?? new JobStatus();
            }
            catch
            {
                return new JobStatus { IsRunning = false, CurrentTask = "Error reading status" };
            }
        }
    }
}
