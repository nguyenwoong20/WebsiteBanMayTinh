using System.ComponentModel.DataAnnotations;

namespace Website_BanMayTinh.Models
{
    public class Brand
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên thương hiệu là bắt buộc")]
        [StringLength(20)] // Không nên để nvarchar(max)
        [RegularExpression(@"^\S.*$", ErrorMessage = "Tên thương hiệu không được bắt đầu bằng khoảng trắng.")]
        [Display(Name = "Tên thương hiệu")]
        public string Name { get; set; }

        public List<Product>? Products { get; set; }
    }
}
