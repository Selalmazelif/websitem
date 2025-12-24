using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MektupProje.Data;
using MektupProje.Services;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// Upload limit (ses dosyası kesilmesin)
builder.Services.Configure<FormOptions>(o =>
{
    o.MultipartBodyLengthLimit = 50 * 1024 * 1024; // 50MB
});

// Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(opt =>
{
    opt.IdleTimeout = TimeSpan.FromMinutes(30);
    opt.Cookie.HttpOnly = true;
    opt.Cookie.IsEssential = true;
});

// DB
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// Identity
builder.Services
    .AddIdentity<AppUser, IdentityRole>(opt =>
    {
        opt.Password.RequiredLength = 6;
        opt.Password.RequireNonAlphanumeric = false;
        opt.Password.RequireUppercase = false;
        opt.Password.RequireLowercase = false;
        opt.Password.RequireDigit = true;
        opt.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(opt =>
{
    opt.LoginPath = "/Hesap/Giris";
    opt.AccessDeniedPath = "/Hesap/ErisimReddedildi";
});

// Admin policy
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireAssertion(ctx =>
        {
            if (ctx.User.Identity?.IsAuthenticated != true) return false;

            var name = ctx.User.Identity?.Name;
            var email = ctx.User.FindFirstValue(ClaimTypes.Email);

            return string.Equals(name, "elifselalmaz@gmail.com", StringComparison.OrdinalIgnoreCase)
                || string.Equals(email, "elifselalmaz@gmail.com", StringComparison.OrdinalIgnoreCase);
        }));
});

// ✅ UploadOptions tek yerden okunacak
builder.Services.Configure<UploadOptions>(builder.Configuration.GetSection("Upload"));

// Services
builder.Services.AddScoped<IFileStorageService, FileStorageService>();
builder.Services.AddScoped<ILetterService, LetterService>();
builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();

builder.Services.AddHttpClient();

// Background worker
builder.Services.AddHostedService<LetterDeliveryHostedService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Hata");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Hesap}/{action=Giris}/{id?}");

app.Run();