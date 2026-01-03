using FootageSearch.Indexer;
using FootageSearch.Indexer.Services;
using FootageSearch.Core.Interfaces;
using FootageSearch.Core.Services;
using FootageSearch.Data;
using FootageSearch.Media.Interfaces;
using FootageSearch.Media.Services;
using FootageSearch.Embeddings.Interfaces;
using FootageSearch.Embeddings.Services;
using FootageSearch.Transcription.Interfaces;
using FootageSearch.Transcription.Services;
using FootageSearch.OCR.Interfaces;
using FootageSearch.OCR.Services;
using FootageSearch.AI.Interfaces;
using FootageSearch.AI.Services;
using FootageSearch.Data.Services;

var builder = Host.CreateApplicationBuilder(args);

// Register Services
builder.Services.AddDbContext<VideoDbContext>();
builder.Services.AddScoped<IJobStatusService>(sp => 
    new DbJobStatusService(sp.GetRequiredService<VideoDbContext>(), "Indexer"));

builder.Services.AddTransient<ISettingsService, JsonSettingsService>();
builder.Services.AddTransient<IMediaService, FfmpegMediaService>();
builder.Services.AddTransient<IVectorDbService, QdrantService>();
builder.Services.AddTransient<ITranscriptionService, WhisperTranscriptionService>();
builder.Services.AddTransient<IOcrService, TesseractOcrService>();
builder.Services.AddTransient<IVisualAiService>(sp => new OllamaVisualAiService("http://localhost:11434", "llama3.2-vision"));
builder.Services.AddTransient<IEmbeddingService>(sp => new OllamaEmbeddingService("http://localhost:11434", "nomic-embed-text"));
builder.Services.AddDbContext<VideoDbContext>();

builder.Services.AddTransient<IndexerService>();

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();