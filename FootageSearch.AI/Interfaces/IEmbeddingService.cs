using System.Threading.Tasks;

namespace FootageSearch.AI.Interfaces
{
    public interface IEmbeddingService
    {
        Task<float[]> GenerateEmbeddingAsync(string text);
    }
}
