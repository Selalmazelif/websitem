using System.ComponentModel.DataAnnotations;

namespace MektupProje.Models.ViewModels;

public class RegisterVm
{
    [Required(ErrorMessage = "E-posta zorunlu.")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta gir.")]
    public string Email { get; set; } = default!;

    [Required(ErrorMessage = "Şifre zorunlu.")]
    [MinLength(6, ErrorMessage = "Şifre en az 6 karakter olmalı.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = default!;

    [Required(ErrorMessage = "Şifre tekrar zorunlu.")]
    [Compare(nameof(Password), ErrorMessage = "Şifreler eşleşmiyor.")]
    [DataType(DataType.Password)]
    public string ConfirmPassword { get; set; } = default!;
}
