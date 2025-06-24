using System.ComponentModel.DataAnnotations;

namespace Website_BanMayTinh.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên danh mục là bắt buộc")]
        [StringLength(25, ErrorMessage = "Tên danh mục không được vượt quá 25 ký tự")]
        [RegularExpression(@"^\S.*$", ErrorMessage = "Tên danh mục không được bắt đầu bằng khoảng trắng.")]
        [Display(Name = "Tên danh mục")]
        public string Name { get; set; }

        public List<Product>? Products { get; set; }
    }
}
