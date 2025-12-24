using System.ComponentModel.DataAnnotations;
using MektupProje.Data;

namespace MektupProje.Models;

public enum ContactType { Email = 0 }
public enum LetterStatus { Pending = 0, Processing = 1, Sent = 2, Failed = 3 }

public class Letter
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public string UserId { get; set; } = default!;
    public AppUser? User { get; set; }

    [Required]
    public ContactType ContactType { get; set; } = ContactType.Email;

    [Required, MaxLength(200)]
    public string ContactValue { get; set; } = default!; // giriş yapan kullanıcının maili

    [MaxLength(400)]
    public string? PhotoPath { get; set; }

    [MaxLength(400)]
    public string? AudioPath { get; set; }

    [MaxLength(400)]
    public string? VideoPath { get; set; } // (foto+ses -> video üretirsek buraya yazacağız)

    [Required]
    public string Text { get; set; } = default!;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime DeliverAtUtc { get; set; }

    public LetterStatus Status { get; set; } = LetterStatus.Pending;
    public DateTime? SentAtUtc { get; set; }
    public string? LastError { get; set; }

    public Guid PublicToken { get; set; } = Guid.NewGuid();
}
