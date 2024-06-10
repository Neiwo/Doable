using System.ComponentModel.DataAnnotations;

namespace Doable.Models
{
    public class ForgotPasswordModel
    {
        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; }
    }
}
