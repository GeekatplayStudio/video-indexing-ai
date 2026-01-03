using System.Threading.Tasks;

namespace FootageSearch.OCR.Interfaces
{
    public interface IOcrService
    {
        Task<string> ExtractTextFromImageAsync(string imagePath);
    }
}
