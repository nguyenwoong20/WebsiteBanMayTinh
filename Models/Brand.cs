using System.ComponentModel.DataAnnotations;

namespace Website_BanMayTinh.Models
{
    public class Brand
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(50)]
        public string Name { get; set; }

        // Một Brand có nhiều Product
        public List<Product>? Products { get; set; }
    }
}
