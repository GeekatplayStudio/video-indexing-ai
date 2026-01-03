using System.Threading.Tasks;

namespace FootageSearch.Transcription.Interfaces
{
    public interface ITranscriptionService
    {
        Task<string> TranscribeAudioAsync(string audioFilePath);
    }
}
