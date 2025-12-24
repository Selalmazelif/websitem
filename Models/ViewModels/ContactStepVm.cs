using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace MektupProje.Models.ViewModels;

public class ContactStepVm
{
    public IFormFile? Photo { get; set; }

    [Range(1, 120, ErrorMessage = "Yıl 1 ile 120 arasında olmalı.")]
    public int? YearsLater { get; set; }

    public bool DemoOneMinute { get; set; } = true;
}
