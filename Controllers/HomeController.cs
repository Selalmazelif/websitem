using Microsoft.AspNetCore.Mvc;

namespace MektupProje.Controllers;

public class HomeController : Controller
{
    public IActionResult Hata() => View();
}
