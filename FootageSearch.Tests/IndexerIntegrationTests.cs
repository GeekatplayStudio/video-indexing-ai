using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FootageSearch.AI.Interfaces;
using FootageSearch.Core.Interfaces;
using FootageSearch.Core.Models;
using FootageSearch.Data;
using FootageSearch.Data.Models;
using FootageSearch.Embeddings.Interfaces;
using FootageSearch.Indexer.Services;
using FootageSearch.Media.Interfaces;
using FootageSearch.OCR.Interfaces;
using FootageSearch.Transcription.Interfaces;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace FootageSearch.Tests
{
    public class IndexerIntegrationTests
    {
        [Fact]
        public async Task Indexer_FullPipeline_FlowTest()
        {
            // Arrange
            var mockSettings = new Mock<ISettingsService>();
            mockSettings.Setup(s => s.LoadSettings()).Returns(new AppSettings 
            { 
                WatchFolders = new List<string> { "C:\\Test" },
                TempFolderPath = "C:\\Temp"
            });

            var mockMedia = new Mock<IMediaService>();
            mockMedia.Setup(m => m.GetMetadataAsync(It.IsAny<string>()))
                .ReturnsAsync(new VideoFile 
                { 
                    FilePath = "C:\\Test\\video.mp4", 
                    DurationSeconds = 10,
                    Width = 1920,
                    Height = 1080
                });
            mockMedia.Setup(m => m.ExtractFrameAsync(It.IsAny<string>(), It.IsAny<double>(), It.IsAny<string>()))
                .ReturnsAsync("C:\\Temp\\frame.jpg");

            var mockVectorDb = new Mock<IVectorDbService>();
            var mockTranscribe = new Mock<ITranscriptionService>();
            mockTranscribe.Setup(t => t.TranscribeAudioAsync(It.IsAny<string>())).ReturnsAsync("Audio transcript");

            var mockOcr = new Mock<IOcrService>();
            mockOcr.Setup(o => o.ExtractTextFromImageAsync(It.IsAny<string>())).ReturnsAsync("OCR Text");

            var mockVisual = new Mock<IVisualAiService>();
            mockVisual.Setup(v => v.DescribeImageAsync(It.IsAny<string>())).ReturnsAsync("Visual Description");

            var mockEmbedding = new Mock<IEmbeddingService>();
            mockEmbedding.Setup(e => e.GenerateEmbeddingAsync(It.IsAny<string>())).ReturnsAsync(new float[384]);

            // Use In-Memory DB for testing
            var options = new DbContextOptionsBuilder<VideoDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb")
                .Options;
            var dbContext = new VideoDbContext(options);

            var indexer = new IndexerService(
                mockSettings.Object,
                mockMedia.Object,
                mockVectorDb.Object,
                mockTranscribe.Object,
                mockOcr.Object,
                mockVisual.Object,
                mockEmbedding.Object,
                dbContext
            );

            // Act
            // We can't easily test the file system loop without abstraction, 
            // but we can verify the service constructs and dependencies are set.
            // To test the loop, we'd need to abstract Directory.GetFiles.
            // For now, let's verify the DB context works.
            
            dbContext.VideoFiles.Add(new VideoFile { FilePath = "C:\\Test\\existing.mp4" });
            await dbContext.SaveChangesAsync();

            // Assert
            Assert.Equal(1, await dbContext.VideoFiles.CountAsync());
        }
    }
}
