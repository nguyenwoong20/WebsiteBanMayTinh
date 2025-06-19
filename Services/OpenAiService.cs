using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Website_BanMayTinh.Services
{
    public class OpenAiService
    {
        private readonly IConfiguration _config;
        private readonly HttpClient _httpClient;

        public OpenAiService(IConfiguration config)
        {
            _config = config;
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _config["OpenAI:ApiKey"]);
        }

        public async Task<string> AskAsync(string prompt, bool isShort = true)
        {
            var systemPrompt = isShort
                ? "Hãy trả lời ngắn gọn, dễ hiểu, không quá 3 câu."
                : "Hãy tư vấn chi tiết nếu người dùng yêu cầu.";

            var body = new
            {
                model = "gpt-3.5-turbo",
                messages = new[]
                {
            new { role = "system", content = systemPrompt },
            new { role = "user", content = prompt }
        }
            };

            var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);
            var responseString = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(responseString);
            return doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
        }
    }
}
