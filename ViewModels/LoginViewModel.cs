using System.ComponentModel.DataAnnotations;

namespace PressTheButton.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Informe um E-Mail válido")]
        [EmailAddress(ErrorMessage = "E-Mail inválido")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Informe a senha para logar com o usuário")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Lembrar-me")]
        public bool RememberMe { get; set; }
    }
}
