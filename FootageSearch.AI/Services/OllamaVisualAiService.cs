using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using FootageSearch.AI.Interfaces;

namespace FootageSearch.AI.Services
{
    public class OllamaVisualAiService : IVisualAiService
    {
        private readonly string _apiUrl;
        private readonly string _model;
        private readonly HttpClient _httpClient;

        public OllamaVisualAiService(string apiUrl, string model)
        {
            _apiUrl = apiUrl.TrimEnd('/');
            _model = model;
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromMinutes(2); // Vision models can be slow
        }

        public async Task<string> DescribeImageAsync(string imagePath)
        {
            if (!File.Exists(imagePath))
            {
                return "Error: Image file not found.";
            }

            try
            {
                byte[] imageBytes = await File.ReadAllBytesAsync(imagePath);
                string base64Image = Convert.ToBase64String(imageBytes);

                var requestData = new
                {
                    model = _model,
                    prompt = "Describe this image in detail. Focus on objects, actions, and setting.",
                    images = new[] { base64Image },
                    stream = false
                };

                string jsonContent = JsonSerializer.Serialize(requestData);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _httpClient.PostAsync($"{_apiUrl}/api/generate", content);
                
                if (!response.IsSuccessStatusCode)
                {
                    return $"Error: Ollama API returned {response.StatusCode}";
                }

                string responseString = await response.Content.ReadAsStringAsync();
                
                using (JsonDocument doc = JsonDocument.Parse(responseString))
                {
                    if (doc.RootElement.TryGetProperty("response", out JsonElement responseElement))
                    {
                        return responseElement.GetString() ?? "No description generated.";
                    }
                }

                return "Error: Could not parse Ollama response.";
            }
            catch (Exception ex)
            {
                return $"Error calling Ollama: {ex.Message}";
            }
        }
    }
}
