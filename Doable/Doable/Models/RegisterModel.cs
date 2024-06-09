using System.ComponentModel.DataAnnotations;

namespace Doable.Models
{
    public class RegisterModel
    {
        [Required]
        [StringLength(50)]
        public string Username { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long.")]
        public string Password { get; set; }

        [Required]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        [StringLength(100)]
        public string ConfirmPassword { get; set; }

        [Required]
        [StringLength(50)]
        public string Role { get; set; }

        [Phone]
        [StringLength(11)]
        public string PhoneNumber { get; set; }

        [StringLength(50)]
        public string CreatedBy { get; set; } // New Field
    }
}
