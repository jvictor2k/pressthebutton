using System.ComponentModel.DataAnnotations;

namespace PressTheButton.ViewModels
{
    public class RegistroViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirme a senha")]
        [Compare("Password", ErrorMessage = "As senhas diferem")]
        public string ConfirmPassword { get; set;}
    }
}
