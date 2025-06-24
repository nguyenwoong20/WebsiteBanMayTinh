using System.ComponentModel.DataAnnotations;

namespace Website_BanMayTinh.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên sản phẩm là bắt buộc")]
        [StringLength(150)]
        [RegularExpression(@"^\S.*$", ErrorMessage = "Tên sản phẩm không được bắt đầu bằng khoảng trắng.")]
        [Display(Name = "Tên sản phẩm")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Giá sản phẩm là bắt buộc")]
        [Range(0.01, 1000000000, ErrorMessage = "Giá sản phẩm phải lớn hơn 0 và nhỏ hơn 1.000.000.000")]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Mô tả là bắt buộc")]
        [StringLength(1500)]
        public string Description { get; set; }

        [StringLength(255)] // Tránh nvarchar(max) không cần thiết
        public string? ImageUrl { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [Required]
        public int BrandId { get; set; }

        public bool IsFeatured { get; set; }

        [Required(ErrorMessage = "Số lượng tồn kho là bắt buộc")]
        [Range(0, 10000, ErrorMessage = "Số lượng tồn kho phải từ 0 đến 10.000")]
        public int Stock { get; set; } = 1;

        // Navigation
        public Category? Category { get; set; }
        public Brand? Brand { get; set; }
        public List<ProductImage>? ProductImages { get; set; }
    }
}
