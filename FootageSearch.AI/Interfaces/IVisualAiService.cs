using System.Threading.Tasks;

namespace FootageSearch.AI.Interfaces
{
    public interface IVisualAiService
    {
        Task<string> DescribeImageAsync(string imagePath);
    }
}
