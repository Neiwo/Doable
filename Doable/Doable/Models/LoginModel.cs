using System.ComponentModel.DataAnnotations;

namespace Doable.Models
{
    public class LoginModel
    {
        [Required]
        [StringLength(50)]
        public string Username { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [StringLength(100)]
        public string Password { get; set; }
    }
}