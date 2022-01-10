using System.ComponentModel.DataAnnotations;


namespace Memento.Models
{


    public class RegisterExternalLoginModel
    {
        [Required]
        [Display(Name = "User name")]
        public string UserName { get; set; }

        public string ExternalLoginData { get; set; }
    }

    public class PasswordModel 
    {
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class RecoverPasswordModel : PasswordModel 
    {
        public string Token { get; set; }
        public string Message { get; set; }
    }

    public class LocalPasswordModel : PasswordModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string OldPassword { get; set; }
    }

    public class RegisterModel
    {

        [Required]
        [Display(Name = "Name")]
        [StringLength(50, ErrorMessage = "Name is required - it is how people know who you are on Fyli.")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Email")]
        [EmailAddress]
        public string Email { get; set; }

        public string ReturnUrl { get; set; }

        public string Token { get; set; }

        public bool AcceptTerms { get; set; }
    }

    public class ForgotPasswordModel
    {
        [Required(ErrorMessage="Email is required.")]
        [Display(Name = "Email")]
        public string Email {get; set;}
    }

    public class ExternalLogin
    {
        public string Provider { get; set; }
        public string ProviderDisplayName { get; set; }
        public string ProviderUserId { get; set; }
    }
}
