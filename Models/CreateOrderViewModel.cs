using System.ComponentModel.DataAnnotations;

namespace Website_BanMayTinh.Models
{
    public class CreateOrderViewModel
    {
        [Required(ErrorMessage = "Tên khách hàng là bắt buộc")]
        [StringLength(47)]
        public string CustomerName { get; set; }

        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string? CustomerEmail { get; set; }

        [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [StringLength(11, ErrorMessage = "Số điện thoại không được vượt quá 11 ký tự")]
        public string? CustomerPhone { get; set; }

        [Required(ErrorMessage = "Địa chỉ giao hàng là bắt buộc")]
        [StringLength(150)]
        public string ShippingAddress { get; set; }

        public string? Notes { get; set; }

        public string? PaymentMethod { get; set; }

        public bool IsPay { get; set; }

        // ID sản phẩm và số lượng
        [Required]
        public List<OrderItemInput> Items { get; set; } = new();
    }

    public class OrderItemInput
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
