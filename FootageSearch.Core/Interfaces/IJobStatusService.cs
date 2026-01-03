using FootageSearch.Core.Models;

namespace FootageSearch.Core.Interfaces
{
    public interface IJobStatusService
    {
        Task UpdateStatusAsync(string task, string detail, double? progress = null);
        Task<JobStatus> GetStatusAsync();
    }
}
