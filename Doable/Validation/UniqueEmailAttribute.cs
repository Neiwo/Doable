using Doable.Data;
using System.ComponentModel.DataAnnotations;
using System.Linq;

public class UniqueEmailAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        var _context = (ApplicationDbContext)validationContext.GetService(typeof(ApplicationDbContext));
        var email = value as string;

        var user = _context.Users.SingleOrDefault(u => u.Email == email);

        if (user != null)
        {
            return new ValidationResult("Email address is already taken.");
        }

        return ValidationResult.Success;
    }
}