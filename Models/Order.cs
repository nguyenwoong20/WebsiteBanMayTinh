using System.ComponentModel.DataAnnotations;

namespace Website_BanMayTinh.Models
{
    public class Order
    {
        public int Id { get; set; }

        public DateTime OrderDate { get; set; }

        [Required]
        [StringLength(150)]
        public string ShippingAddress { get; set; }

        public decimal TotalAmount { get; set; }

        public string? Notes { get; set; }

        [Required]
        public string UserId { get; set; }

        public bool IsPay { get; set; }

        public ApplicationUser? User { get; set; }

        public List<OrderDetail>? OrderDetails { get; set; }

        public string? PaymentMethod { get; set; }

        public string? ReceiptImagePath { get; set; }

    }
}
