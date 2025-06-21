using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Website_BanMayTinh.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        public string FullName { get; set; }
        public string Address { get; set; }
        [StringLength(3)]
        public string Age { get; set; }
        [StringLength(11)]
        public string PhoneNumber { get; set; }
    }
}
