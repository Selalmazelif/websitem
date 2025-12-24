using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MektupProje.Data;

namespace MektupProje.Controllers;

public class MektupController : Controller
{
    private readonly AppDbContext _db;

    public MektupController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet("Mektup/Izle/{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> Izle(Guid id)
    {
        var letter = await _db.Letters.FirstOrDefaultAsync(x => x.PublicToken == id);
        if (letter is null) return NotFound();

        return View(letter);
    }
}
