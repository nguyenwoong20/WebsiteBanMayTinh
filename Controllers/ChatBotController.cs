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

            // Thêm tin nhắn người dùng
            history.Add(new ChatMessage
            {
                Role = "user",
                Message = prompt,
                Timestamp = DateTime.Now
            });

            var products = _context.Products
                .Select(p => $"{p.Name} - Giá: {p.Price} VNĐ")
                .ToList();

                        string productList = string.Join("\n", products);

            // Prompt hệ thống
            string systemPrompt = @$"
            Bạn là một trợ lý AI tư vấn cấu hình máy tính tại website LV Computer.

            🎯 Mục tiêu:
            - Tư vấn cấu hình máy tính cho người dùng theo **ngân sách** hoặc **nhu cầu cụ thể** (học tập, chơi game, đồ họa...).
            - Sử dụng **chính xác các sản phẩm trong danh sách bên dưới**. Không được bịa ra linh kiện, giá, thương hiệu hoặc thông tin không tồn tại.
            - Nếu câu hỏi **không rõ ràng** có thể hỏi lại khách hàng, không trả lời đại theo quán tính. Biết rõ ý muốn câu hỏi là gì rồi mới trả lời
            - Tính đúng tổng giá nhất với các sản phẩm có trong cơ sở dữ liệu

            📌 Quy tắc tư vấn:
            1. Chỉ sử dụng linh kiện từ danh sách sau (đây là dữ liệu thực tế từ kho hàng):
            {productList}

            2. Các linh kiện phải **tương thích** với nhau:
               - CPU AMD → Mainboard Socket AM4/AM5
               - CPU Intel → Mainboard LGA 1200 hoặc LGA 1700
               - RAM DDR4/DDR5 phải tương thích với mainboard
               - Nguồn đủ công suất cho GPU và hệ thống
               - Case và mainboard khớp chuẩn ATX/mATX nếu có liên quan

            3. ✅ Phải **tính đúng tổng chi phí** của cấu hình. Nếu vượt ngân sách, hãy đề xuất điều chỉnh.

            4. Nếu **không thể tạo cấu hình hợp lệ**, hãy trả lời: “Không tìm thấy cấu hình phù hợp với yêu cầu và dữ liệu hiện có.”

            🚫 Không trả lời bất kỳ chủ đề nào ngoài việc tư vấn cấu hình PC.

            Hãy luôn rõ ràng, dễ hiểu, và ưu tiên đề xuất tối ưu trong ngân sách người dùng.";

            // Ghép với câu hỏi khách hàng
            string finalPrompt = systemPrompt + "\n\nCâu hỏi của khách hàng: " + prompt;

            // Gọi OpenAI
            var response = await _openAi.AskAsync(finalPrompt, isShort: true);

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
            try
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
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}



