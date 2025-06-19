using Microsoft.AspNetCore.Mvc;
using Website_BanMayTinh.Extentions;
using Website_BanMayTinh.Models;
using Website_BanMayTinh.Services;

namespace Website_BanMayTinh.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatBotController : Controller
    {
        private readonly OpenAiService _openAi;
        private readonly ApplicationDbContext _context;

        public ChatBotController(OpenAiService openAi, ApplicationDbContext context)
        {
            _openAi = openAi;
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> Ask([FromBody] string prompt)
        {
            // Lấy lịch sử từ session hoặc tạo mới nếu chưa có
            var history = HttpContext.Session.GetObjectFromJson<List<ChatMessage>>("chatHistory") ?? new List<ChatMessage>();

            // Thêm câu hỏi người dùng mới vào lịch sử
            history.Add(new ChatMessage
            {
                Role = "user",
                Message = prompt,
                Timestamp = DateTime.Now
            });

            // Danh sách sản phẩm có sẵn trong database
            var productList = _context.Products
                .Select(p => $"{p.Name} - Giá: {p.Price:N0} VNĐ")
                .ToList();

            string systemPrompt = @$"
🧠 Bạn là một trợ lý AI tư vấn cấu hình máy tính tại website **LV Computer**.

🔒 Quy tắc bắt buộc:
1. Chỉ tư vấn dựa trên danh sách sản phẩm dưới đây. Không bịa đặt thương hiệu, tên linh kiện hay giá.
2. Tư vấn cấu hình phải đảm bảo linh kiện **tương thích** (VD: CPU AMD ↔ Mainboard AM4/AM5).
3. Nếu không đủ linh kiện phù hợp, hãy trả lời: “Không tìm thấy cấu hình phù hợp từ dữ liệu hiện có.”
4. Không trả lời các câu hỏi không liên quan đến cấu hình máy tính (VD: thời tiết, tin tức...).
5. Nếu người dùng hỏi mơ hồ (ví dụ “20tr”), hãy hỏi lại rõ mục đích sử dụng.
6. Khi tính tổng giá, phải **cộng chính xác giá từng linh kiện**, không sai lệch.
7. Chỉ trả lời bằng **tiếng Việt**.

📦 Danh sách sản phẩm:
{string.Join("\n", productList)}
";

            // Tạo lịch sử hội thoại cho GPT: gồm systemPrompt + lịch sử trò chuyện
            var messageHistory = new List<object>
            {
                new { role = "system", content = systemPrompt }
            };

            messageHistory.AddRange(history.Select(h => new
            {
                role = h.Role == "user" ? "user" : "assistant",
                content = h.Message
            }));

            // Gửi toàn bộ lịch sử vào GPT
            var response = await _openAi.AskAsync(messageHistory);

            // Lưu phản hồi lại vào session
            history.Add(new ChatMessage
            {
                Role = "bot",
                Message = response,
                Timestamp = DateTime.Now
            });

            HttpContext.Session.SetObjectAsJson("chatHistory", history);

            return Ok(new { reply = response });
        }

        [HttpGet("history")]
        public IActionResult GetHistory()
        {
            var history = HttpContext.Session.GetObjectFromJson<List<ChatMessage>>("chatHistory") ?? new List<ChatMessage>();

            if (history.Count == 0)
            {
                history.Add(new ChatMessage
                {
                    Role = "bot",
                    Message = "👋 Xin chào! Tôi là trợ lý AI của LV Computer. Bạn muốn tư vấn cấu hình học tập, chơi game hay dựng video?",
                    Timestamp = DateTime.Now
                });

                HttpContext.Session.SetObjectAsJson("chatHistory", history);
            }

            return Ok(history);
        }
    }
}
