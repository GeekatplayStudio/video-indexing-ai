using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using FootageSearch.AI.Interfaces;

namespace FootageSearch.AI.Services
{
    public class OllamaEmbeddingService : IEmbeddingService
    {
        private readonly string _apiUrl;
        private readonly string _model;
        private readonly HttpClient _httpClient;

        public OllamaEmbeddingService(string apiUrl, string model)
        {
            _apiUrl = apiUrl.TrimEnd('/');
            _model = model;
            _httpClient = new HttpClient();
        }

        public async Task<float[]> GenerateEmbeddingAsync(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return new float[0];

            try
            {
                var requestData = new
                {
                    model = _model,
                    prompt = text
                };

                string jsonContent = JsonSerializer.Serialize(requestData);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _httpClient.PostAsync($"{_apiUrl}/api/embeddings", content);
                
                if (!response.IsSuccessStatusCode)
                {
                    // Fallback or throw? For now, return empty to avoid crashing, but log internally if possible
                    Console.WriteLine($"Error calling Ollama Embeddings: {response.StatusCode}");
                    return new float[0];
                }

                string responseString = await response.Content.ReadAsStringAsync();
                
                using (JsonDocument doc = JsonDocument.Parse(responseString))
                {
                    if (doc.RootElement.TryGetProperty("embedding", out JsonElement embeddingElement))
                    {
                        var embedding = new float[embeddingElement.GetArrayLength()];
                        int i = 0;
                        foreach (var val in embeddingElement.EnumerateArray())
                        {
                            embedding[i++] = (float)val.GetDouble();
                        }
                        return embedding;
                    }
                }

                return new float[0];
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating embedding: {ex.Message}");
                return new float[0];
            }
        }
    }
}
