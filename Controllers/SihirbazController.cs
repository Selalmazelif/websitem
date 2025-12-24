using MektupProje.Data;
using MektupProje.Models;
using MektupProje.Models.ViewModels;
using MektupProje.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MektupProje.Controllers;

[Authorize]
public class SihirbazController : Controller
{
    private readonly IFileStorageService _files;
    private readonly ILetterService _letters;
    private readonly UserManager<AppUser> _userManager;

    private const string SessPhotoPath = "wiz_photoPath";
    private const string SessDeliverAtUtc = "wiz_deliverAtUtc";
    private const string SessAudioPath = "wiz_audioPath";

    public SihirbazController(IFileStorageService files, ILetterService letters, UserManager<AppUser> userManager)
    {
        _files = files;
        _letters = letters;
        _userManager = userManager;
    }

    [HttpGet]
    public IActionResult Iletisim()
    {
        return View(new ContactStepVm { DemoOneMinute = true });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Iletisim(ContactStepVm vm, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(vm);

        var user = await _userManager.GetUserAsync(User);
        var userEmail = user?.Email;

        if (string.IsNullOrWhiteSpace(userEmail))
        {
            ModelState.AddModelError("", "Hesabınıza ait e-posta bulunamadı. Lütfen tekrar giriş yapın.");
            return View(vm);
        }

        string? photoPath = null;
        if (vm.Photo is not null)
            photoPath = await _files.SavePhotoAsync(vm.Photo, ct);

        DateTime deliverAtUtc;
        if (vm.DemoOneMinute)
        {
            deliverAtUtc = DateTime.UtcNow.AddMinutes(1);
        }
        else
        {
            if (vm.YearsLater is null)
            {
                ModelState.AddModelError(nameof(vm.YearsLater), "Kaç yıl sonra gönderileceğini yaz.");
                return View(vm);
            }
            deliverAtUtc = DateTime.UtcNow.AddYears(vm.YearsLater.Value);
        }

        if (!string.IsNullOrWhiteSpace(photoPath))
            HttpContext.Session.SetString(SessPhotoPath, photoPath);

        HttpContext.Session.SetString(SessDeliverAtUtc, deliverAtUtc.ToString("O"));

        return RedirectToAction("Mektup");
    }

    [HttpGet]
    public IActionResult Mektup()
    {
        return View(new LetterStepVm());
    }

    // ✅ Ses upload: MediaRecorder buraya dosya yollar.
    // ✅ Hata yakalayıp gerçek nedeni JSON ile döndürüyoruz.
    [HttpPost]
    public async Task<IActionResult> SesYukle(IFormFile audio, CancellationToken ct)
    {
        try
        {
            if (audio is null) return BadRequest(new { error = "Ses dosyası yok." });

            var audioPath = await _files.SaveAudioAsync(audio, ct);
            HttpContext.Session.SetString(SessAudioPath, audioPath);

            return Ok(new { path = audioPath });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                error = ex.Message,
                contentType = audio?.ContentType,
                fileName = audio?.FileName,
                length = audio?.Length
            });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Mektup(LetterStepVm vm, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(vm);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return RedirectToAction("Giris", "Hesap");

        var user = await _userManager.GetUserAsync(User);
        var userEmail = user?.Email;
        if (string.IsNullOrWhiteSpace(userEmail))
            return RedirectToAction("Iletisim");

        var deliverStr = HttpContext.Session.GetString(SessDeliverAtUtc);
        if (string.IsNullOrWhiteSpace(deliverStr))
            return RedirectToAction("Iletisim");

        var deliverAtUtc = DateTime.Parse(deliverStr, null, System.Globalization.DateTimeStyles.RoundtripKind);

        var photoPath = HttpContext.Session.GetString(SessPhotoPath);
        var audioPath = HttpContext.Session.GetString(SessAudioPath);

        await _letters.CreateLetterAsync(
            userId: userId,
            contactType: ContactType.Email,
            contactValue: userEmail,
            photoPath: photoPath,
            audioPath: audioPath,
            text: vm.Text,
            deliverAtUtc: deliverAtUtc,
            ct: ct);

        HttpContext.Session.Remove(SessPhotoPath);
        HttpContext.Session.Remove(SessDeliverAtUtc);
        HttpContext.Session.Remove(SessAudioPath);

        return RedirectToAction("Kaydedildi");
    }

    [HttpGet]
    public IActionResult Kaydedildi() => View();
}
