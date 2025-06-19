using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace Website_BanMayTinh.Services
{
    public class OpenAiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public OpenAiService(IConfiguration config)
        {
            _httpClient = new HttpClient();
            _apiKey = config["OpenAI:ApiKey"];
        }

        public async Task<string> AskAsync(List<object> messages)
        {
            var request = new
            {
                model = "gpt-3.5-turbo", // hoặc gpt-4 nếu bạn có quyền
                messages = messages,
                temperature = 0.7
            };

            var requestContent = new StringContent(
                JsonSerializer.Serialize(request),
                Encoding.UTF8,
                "application/json"
            );

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

            var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", requestContent);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"OpenAI API error: {error}");
            }

            var responseString = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseString);
            var result = doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();

            return result?.Trim() ?? "❌ Không nhận được phản hồi từ AI.";
        }
    }
}

