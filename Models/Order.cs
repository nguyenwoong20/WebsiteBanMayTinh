using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Website_BanMayTinh.Models
{
    public class Order
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.Now;
        public List<OrderDetail> OrderDetails { get; set; } = new();
        public string ShippingAddress { get; set; } 

        public decimal TotalAmount { get; set; }
        public string? Notes { get; set; }

        [ForeignKey("User")]
        public string UserId { get; set; } = null!;
        public ApplicationUser User { get; set; } = null!;

        public bool IsPay { get; set; } = false;
    }
}
