namespace Website_BanMayTinh.Models
{
    public class ChatMessage
    {
        public string Role { get; set; } // "user" hoặc "bot"
        public string Message { get; set; }
        public DateTime Timestamp { get; set; }

    }
}
