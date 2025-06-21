using System.ComponentModel.DataAnnotations;

namespace Website_BanMayTinh.Models
{
    public class ProductImage
    {
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string Url { get; set; }

        public int ProductId { get; set; }

        public Product? Product { get; set; }
    }
}
