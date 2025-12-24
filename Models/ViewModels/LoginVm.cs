using System.ComponentModel.DataAnnotations;

namespace MektupProje.Models.ViewModels;

public class LoginVm
{
    [Required(ErrorMessage = "E-posta zorunlu.")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta gir.")]
    public string Email { get; set; } = default!;

    [Required(ErrorMessage = "Şifre zorunlu.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = default!;
}
