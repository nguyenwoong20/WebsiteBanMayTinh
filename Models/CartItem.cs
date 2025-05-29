namespace Website_BanMayTinh.Models
{
    public class CartItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }
        //lưu số lượng
        public int Quantity { get; set; }
    }
}
