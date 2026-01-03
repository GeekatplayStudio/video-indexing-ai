using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FootageSearch.Data.Models;
using FootageSearch.Media.Interfaces;

namespace FootageSearch.Media.Services
{
    public class FfmpegMediaService : IMediaService
    {
        public async Task<VideoFile> GetMetadataAsync(string filePath)
        {
            var probe = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "ffprobe",
                    Arguments = $"-v quiet -print_format json -show_format -show_streams \"{filePath}\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            probe.Start();
            
            // Add timeout for ffprobe (30 seconds)
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            try
            {
                await probe.WaitForExitAsync(cts.Token);
            }
            catch (OperationCanceledException)
            {
                probe.Kill();
                throw new TimeoutException($"ffprobe timed out while processing {filePath}");
            }

            var json = await probe.StandardOutput.ReadToEndAsync();
            
            using var doc = JsonDocument.Parse(json);
            var format = doc.RootElement.GetProperty("format");
            var streams = doc.RootElement.GetProperty("streams");
            
            var videoStream = streams[0]; // Simplification: assume first stream is video or iterate to find codec_type=video
            foreach (var stream in streams.EnumerateArray())
            {
                if (stream.TryGetProperty("codec_type", out var type) && type.GetString() == "video")
                {
                    videoStream = stream;
                    break;
                }
            }

            var duration = format.GetProperty("duration").GetString();
            var width = videoStream.TryGetProperty("width", out var w) ? w.GetInt32() : 0;
            var height = videoStream.TryGetProperty("height", out var h) ? h.GetInt32() : 0;

            return new VideoFile
            {
                FilePath = filePath,
                FileName = Path.GetFileName(filePath),
                DurationSeconds = double.Parse(duration ?? "0"),
                Width = width,
                Height = height,
                Format = format.GetProperty("format_name").GetString() ?? "unknown"
            };
        }

        public async Task<string> ExtractFrameAsync(string filePath, double timeSeconds, string outputFolder)
        {
            var fileName = $"{Path.GetFileNameWithoutExtension(filePath)}_{timeSeconds}.jpg";
            var outputPath = Path.Combine(outputFolder, fileName);

            var ffmpeg = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "ffmpeg",
                    Arguments = $"-ss {timeSeconds} -i \"{filePath}\" -frames:v 1 -q:v 2 \"{outputPath}\" -y",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            ffmpeg.Start();

            // Add timeout for frame extraction (60 seconds)
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(60));
            try
            {
                await ffmpeg.WaitForExitAsync(cts.Token);
            }
            catch (OperationCanceledException)
            {
                ffmpeg.Kill();
                throw new TimeoutException($"ffmpeg timed out while extracting frame from {filePath}");
            }

            return outputPath;
        }
    }
}
