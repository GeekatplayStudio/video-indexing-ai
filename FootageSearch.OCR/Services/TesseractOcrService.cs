using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using FootageSearch.OCR.Interfaces;
using Tesseract;

namespace FootageSearch.OCR.Services
{
    public class TesseractOcrService : IOcrService
    {
        private readonly string _tessDataPath;

        public TesseractOcrService()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            _tessDataPath = Path.Combine(appData, "FootageSearch", "tessdata");
            Directory.CreateDirectory(_tessDataPath);
        }

        public async Task<string> ExtractTextFromImageAsync(string imagePath)
        {
            try
            {
                // 1. Ensure TessData Exists
                var engDataPath = Path.Combine(_tessDataPath, "eng.traineddata");
                if (!File.Exists(engDataPath))
                {
                    using var client = new HttpClient();
                    var data = await client.GetByteArrayAsync("https://github.com/tesseract-ocr/tessdata_fast/raw/main/eng.traineddata");
                    await File.WriteAllBytesAsync(engDataPath, data);
                }

                // 2. Perform OCR
                using var engine = new TesseractEngine(_tessDataPath, "eng", EngineMode.Default);
                using var img = Pix.LoadFromFile(imagePath);
                using var page = engine.Process(img);
                
                var text = page.GetText();
                return text?.Trim() ?? "";
            }
            catch (Exception ex)
            {
                return $"Error performing OCR: {ex.Message}";
            }
        }
    }
}
