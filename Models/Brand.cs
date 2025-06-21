using System.ComponentModel.DataAnnotations;

namespace Website_BanMayTinh.Models
{
    public class Brand
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)] // Không nên để nvarchar(max)
        public string Name { get; set; }

        public List<Product>? Products { get; set; }
    }
}
