using FootageSearch.Core.Interfaces;
using FootageSearch.Core.Models;
using FootageSearch.Data;
using FootageSearch.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace FootageSearch.Data.Services
{
    public class DbJobStatusService : IJobStatusService
    {
        private readonly VideoDbContext _dbContext;
        private readonly string _serviceName;

        public DbJobStatusService(VideoDbContext dbContext, string serviceName = "Indexer")
        {
            _dbContext = dbContext;
            _serviceName = serviceName;
        }

        public async Task UpdateStatusAsync(string task, string detail, double? progress = null)
        {
            var status = await _dbContext.JobStatuses
                .FirstOrDefaultAsync(s => s.ServiceName == _serviceName);

            if (status == null)
            {
                status = new JobStatusEntity { ServiceName = _serviceName };
                _dbContext.JobStatuses.Add(status);
            }

            status.IsRunning = true;
            status.CurrentTask = task;
            status.Detail = detail;
            status.ProgressPercentage = progress;
            status.LastUpdated = DateTime.Now;

            await _dbContext.SaveChangesAsync();
        }

        public async Task<JobStatus> GetStatusAsync()
        {
            var status = await _dbContext.JobStatuses
                .FirstOrDefaultAsync(s => s.ServiceName == _serviceName);

            if (status == null)
            {
                return new JobStatus { IsRunning = false };
            }

            // Check if stale (e.g., older than 1 minute)
            if (DateTime.Now - status.LastUpdated > TimeSpan.FromMinutes(1))
            {
                status.IsRunning = false;
                status.CurrentTask = "Offline";
                status.Detail = "Service not responding";
            }

            return new JobStatus
            {
                IsRunning = status.IsRunning,
                CurrentTask = status.CurrentTask,
                Detail = status.Detail,
                ProgressPercentage = status.ProgressPercentage,
                LastUpdated = status.LastUpdated
            };
        }
    }
}
