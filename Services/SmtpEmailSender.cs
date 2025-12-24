using System.Net;
using System.Net.Mail;

namespace MektupProje.Services;

public class SmtpEmailSender : IEmailSender
{
    private readonly IConfiguration _cfg;

    public SmtpEmailSender(IConfiguration cfg) => _cfg = cfg;

    public async Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken ct)
    {
        var host = (_cfg["Email:SmtpHost"] ?? "").Trim();
        var portStr = (_cfg["Email:SmtpPort"] ?? "").Trim();
        var user = (_cfg["Email:SmtpUser"] ?? "").Trim();
        var pass = (_cfg["Email:SmtpPass"] ?? "").Trim();
        var fromEmail = (_cfg["Email:FromEmail"] ?? "").Trim();
        var fromName = (_cfg["Email:FromName"] ?? "Geleceğe Mektup").Trim();

        if (string.IsNullOrWhiteSpace(host) ||
            string.IsNullOrWhiteSpace(user) ||
            string.IsNullOrWhiteSpace(pass) ||
            string.IsNullOrWhiteSpace(fromEmail))
            throw new InvalidOperationException("SMTP ayarları eksik (appsettings.json -> Email).");

        if (!int.TryParse(portStr, out var port)) port = 587;

        using var msg = new MailMessage
        {
            From = new MailAddress(fromEmail, fromName),
            Subject = subject ?? "",
            Body = htmlBody ?? "",
            IsBodyHtml = true
        };
        msg.To.Add(toEmail);

        using var client = new SmtpClient(host, port)
        {
            EnableSsl = true,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(user, pass),
            DeliveryMethod = SmtpDeliveryMethod.Network,
            Timeout = 20000
        };

        await Task.Run(() => client.Send(msg), ct);
    }
}
