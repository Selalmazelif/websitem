using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MektupProje.Data;

namespace MektupProje.Controllers;

[Authorize(Policy = "AdminOnly")]
public class AdminController : Controller
{
    private readonly AppDbContext _db;
    public AdminController(AppDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        var letters = await _db.Letters
            .Include(x => x.User)
            .OrderByDescending(x => x.CreatedAtUtc)
            .Take(500)
            .ToListAsync();

        return View(letters);
    }

    public async Task<IActionResult> Detay(Guid id)
    {
        var letter = await _db.Letters.Include(x => x.User).FirstOrDefaultAsync(x => x.Id == id);
        if (letter is null) return NotFound();
        return View(letter);
    }
}
