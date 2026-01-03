using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FootageSearch.Core.Interfaces;
using FootageSearch.Data;
using FootageSearch.Data.Models;
using FootageSearch.Embeddings.Interfaces;
using FootageSearch.Media.Interfaces;
using FootageSearch.Transcription.Interfaces;
using FootageSearch.OCR.Interfaces;
using FootageSearch.AI.Interfaces;

namespace FootageSearch.Indexer.Services
{
    public class IndexerService
    {
        private readonly ISettingsService _settingsService;
        private readonly IMediaService _mediaService;
        private readonly IVectorDbService _vectorDbService;
        private readonly ITranscriptionService _transcriptionService;
        private readonly IOcrService _ocrService;
        private readonly IVisualAiService _visualAiService;
        private readonly IEmbeddingService _embeddingService;
        private readonly VideoDbContext _dbContext;

        public IndexerService(
            ISettingsService settingsService,
            IMediaService mediaService,
            IVectorDbService vectorDbService,
            ITranscriptionService transcriptionService,
            IOcrService ocrService,
            IVisualAiService visualAiService,
            IEmbeddingService embeddingService,
            VideoDbContext dbContext)
        {
            _settingsService = settingsService;
            _mediaService = mediaService;
            _vectorDbService = vectorDbService;
            _transcriptionService = transcriptionService;
            _ocrService = ocrService;
            _visualAiService = visualAiService;
            _embeddingService = embeddingService;
            _dbContext = dbContext;
        }

        public async Task ReIndexAsync(IProgress<string> progress)
        {
            var settings = _settingsService.LoadSettings();
            var logPath = Path.Combine(settings.TempFolderPath, $"indexing_log_{DateTime.Now:yyyyMMdd_HHmmss}.txt");
            Directory.CreateDirectory(settings.TempFolderPath);

            void Log(string message)
            {
                progress.Report(message);
                try { File.AppendAllText(logPath, $"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}"); } catch { }
            }

            Log($"[System] Starting Indexing. Log file: {logPath}");
            Log("[System] Initializing Database...");
            await _dbContext.Database.EnsureCreatedAsync();
            
            Log("[System] Initializing Vector DB...");
            try 
            {
                await _vectorDbService.InitializeAsync();
            }
            catch (Exception ex)
            {
                Log($"[Warning] Vector DB not available: {ex.Message}");
            }

            var extensions = new[] { ".mp4", ".mov", ".avi", ".mkv" };

            foreach (var folder in settings.WatchFolders)
            {
                if (!Directory.Exists(folder)) 
                {
                    Log($"[Error] Watch folder not found: {folder}");
                    continue;
                }

                var files = Directory.GetFiles(folder, "*.*", SearchOption.AllDirectories)
                    .Where(f => extensions.Contains(Path.GetExtension(f).ToLower()));

                foreach (var file in files)
                {
                    var fileName = Path.GetFileName(file);
                    if (_dbContext.VideoFiles.Any(v => v.FilePath == file))
                    {
                        Log($"[Skip] Already indexed: {fileName}");
                        continue;
                    }

                    Log($"[Info] Processing: {fileName}");

                    try
                    {
                        // 1. Extract Metadata
                        Log($"  > Extracting metadata...");
                        var video = await _mediaService.GetMetadataAsync(file);
                        Log($"    - Duration: {video.DurationSeconds}s");
                        Log($"    - Resolution: {video.Width}x{video.Height}");
                        Log($"    - Format: {video.Format}");

                        // 2. Transcription
                        Log($"  > Generating transcript...");
                        
                        // Create a progress wrapper that logs to our file
                        var transProgress = new Progress<string>(msg => Log($"    [Whisper] {msg}"));
                        
                        video.Transcript = await _transcriptionService.TranscribeAudioAsync(file, transProgress);
                        Log($"    - Transcript length: {video.Transcript.Length} chars");

                        // 3. Frame Extraction & Visual AI
                        Log($"  > Extracting keyframe for Visual AI & OCR...");
                        try 
                        {
                            var framePath = await _mediaService.ExtractFrameAsync(file, video.DurationSeconds / 2, settings.TempFolderPath);
                            
                            // OCR
                            Log($"    [OCR] Analyzing text in frame...");
                            video.OcrText = await _ocrService.ExtractTextFromImageAsync(framePath);
                            Log($"    - OCR Text found: {video.OcrText}");

                            // Visual Description
                            Log($"    [Visual AI] Analyzing image content (this may take a moment)...");
                            video.VisualDescription = await _visualAiService.DescribeImageAsync(framePath);
                            Log($"    - Visual Description: {video.VisualDescription}");
                            
                            // Cleanup frame if needed, or keep for thumbnail
                        }
                        catch (Exception ex)
                        {
                            Log($"    [Warning] Visual AI/OCR failed: {ex.Message}");
                        }

                        // 4. Save to SQLite
                        Log($"  > Saving to local database...");
                        _dbContext.VideoFiles.Add(video);
                        await _dbContext.SaveChangesAsync();

                        // 5. Generate Embedding
                        Log($"  > Generating AI Embeddings...");
                        
                        // Combine text for embedding
                        var combinedText = $"{video.FileName} {video.Transcript} {video.OcrText} {video.VisualDescription}";
                        var vector = await _embeddingService.GenerateEmbeddingAsync(combinedText);
                        
                        if (vector.Length == 0)
                        {
                             Log($"    [Warning] Embedding generation failed (empty vector). Skipping vector DB.");
                        }
                        else
                        {
                            try 
                            {
                                await _vectorDbService.UpsertAsync(video.VectorId, vector, new Dictionary<string, string>
                                {
                                    { "path", video.FilePath },
                                    { "filename", video.FileName },
                                    { "transcript", video.Transcript },
                                    { "ocr", video.OcrText },
                                    { "visual_description", video.VisualDescription }
                                });
                                Log($"    - Vector stored in Qdrant (ID: {video.VectorId})");
                            }
                            catch (Exception ex)
                            {
                                Log($"    [Warning] Failed to store vector: {ex.Message}");
                            }
                        }
                        
                        Log($"[Success] Indexed: {fileName}");
                    }
                    catch (Exception ex)
                    {
                        Log($"[Error] Failed {fileName}: {ex.Message}");
                    }
                }
            }
            Log("[System] Indexing Complete.");
        }
    }
}
