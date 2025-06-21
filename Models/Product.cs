using System.ComponentModel.DataAnnotations;

namespace Website_BanMayTinh.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        public decimal Price { get; set; }

        [Required]
        [StringLength(1500)]
        public string Description { get; set; }

        [StringLength(255)] // Tránh nvarchar(max) không cần thiết
        public string? ImageUrl { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [Required]
        public int BrandId { get; set; }

        public bool IsFeatured { get; set; }

        public int Stock { get; set; } = 1;

        // Navigation
        public Category? Category { get; set; }
        public Brand? Brand { get; set; }
        public List<ProductImage>? ProductImages { get; set; }
    }
}
