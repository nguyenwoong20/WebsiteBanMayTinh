using System.ComponentModel.DataAnnotations;

namespace Website_BanMayTinh.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }
        [Required, StringLength(50)]
        public string Name { get; set; }
        public List<Product>? Products { get; set; }
    }
}
