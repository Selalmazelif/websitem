using System.ComponentModel.DataAnnotations;

namespace MektupProje.Models.ViewModels;

public class LetterStepVm
{
    [Required(ErrorMessage = "Mektup metni zorunlu.")]
    [MinLength(5, ErrorMessage = "Mektup çok kısa.")]
    public string Text { get; set; } = default!;

    // Audio upload endpoint'inden dönen path session'da tutulur.
}
