using System.ComponentModel.DataAnnotations;

namespace Restaurant.Models
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
