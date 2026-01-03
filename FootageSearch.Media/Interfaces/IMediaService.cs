using System.Threading.Tasks;
using FootageSearch.Data.Models;

namespace FootageSearch.Media.Interfaces
{
    public interface IMediaService
    {
        Task<VideoFile> GetMetadataAsync(string filePath);
        Task<string> ExtractFrameAsync(string filePath, double timeSeconds, string outputFolder);
    }
}
