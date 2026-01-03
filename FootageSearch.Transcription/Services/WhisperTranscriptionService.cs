using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FootageSearch.Transcription.Interfaces;
using Whisper.net;
using Whisper.net.Ggml;
using System.Diagnostics;

namespace FootageSearch.Transcription.Services
{
    public class WhisperTranscriptionService : ITranscriptionService
    {
        private readonly string _modelsPath;
        private readonly string _tempPath;

        public WhisperTranscriptionService()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            _modelsPath = Path.Combine(appData, "FootageSearch", "Models");
            _tempPath = Path.Combine(appData, "FootageSearch", "Temp");
            
            Directory.CreateDirectory(_modelsPath);
            Directory.CreateDirectory(_tempPath);
        }

        public async Task<string> TranscribeAudioAsync(string videoFilePath, IProgress<string>? progress = null)
        {
            try
            {
                // 1. Ensure Model Exists
                var modelPath = Path.Combine(_modelsPath, "ggml-base.bin");
                if (!File.Exists(modelPath))
                {
                    progress?.Report("Downloading Whisper model...");
                    using var httpClient = new HttpClient();
                    var downloader = new WhisperGgmlDownloader(httpClient);
                    using var stream = await downloader.GetGgmlModelAsync(GgmlType.Base);
                    using var modelStream = File.Create(modelPath);
                    await stream.CopyToAsync(modelStream);
                }

                // 2. Extract Audio using FFmpeg
                progress?.Report("Extracting audio...");
                var audioPath = Path.Combine(_tempPath, $"{Path.GetFileNameWithoutExtension(videoFilePath)}_{Guid.NewGuid()}.wav");
                
                // FFmpeg command to extract 16kHz mono wav
                var startInfo = new ProcessStartInfo
                {
                    FileName = "ffmpeg",
                    Arguments = $"-i \"{videoFilePath}\" -ar 16000 -ac 1 -c:a pcm_s16le \"{audioPath}\" -y",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(startInfo))
                {
                    // Wait max 5 minutes for audio extraction
                    var cts = new CancellationTokenSource(TimeSpan.FromMinutes(5));
                    try
                    {
                        await process.WaitForExitAsync(cts.Token);
                    }
                    catch (OperationCanceledException)
                    {
                        process.Kill();
                        return "Error: Audio extraction timed out.";
                    }
                }

                if (!File.Exists(audioPath))
                {
                    return "Error: Audio extraction failed (ffmpeg might be missing or file invalid).";
                }

                // 3. Transcribe
                progress?.Report("Starting transcription...");
                using var whisperFactory = WhisperFactory.FromPath(modelPath);
                using var processor = whisperFactory.CreateBuilder()
                    .WithLanguage("auto")
                    .Build();

                using var fileStream = File.OpenRead(audioPath);
                var transcript = "";
                
                int segmentCount = 0;
                await foreach (var segment in processor.ProcessAsync(fileStream))
                {
                    transcript += segment.Text + " ";
                    segmentCount++;
                    if (segmentCount % 5 == 0)
                    {
                        progress?.Report($"Transcribing... ({segment.Start} - {segment.End})");
                    }
                }

                // Cleanup
                try { File.Delete(audioPath); } catch { }

                return transcript.Trim();
            }
            catch (Exception ex)
            {
                return $"Error transcribing: {ex.Message}";
            }
        }
    }
}
