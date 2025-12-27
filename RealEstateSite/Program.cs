using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using RealEstateSite.Data;

var builder = WebApplication.CreateBuilder(args);

// 1. Veritabaný Baðlantýsý (Senin kodunda zaten vardýr, burayý kendi ConnectionString'inle ayný býrak)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Authentication (Giriþ) Servisi
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login"; // Giriþ yapmamýþsa buraya at
        options.AccessDeniedPath = "/Account/Login"; // Yetkisi yetmiyorsa buraya at
    });

// 3. Authorization (Yetki) Servisi - ÝÞTE EKSÝK OLAN SATIR BU!
builder.Services.AddAuthorization();

// MVC Servisleri
builder.Services.AddControllersWithViews();

var app = builder.Build();

// ----------------------------------------------------------------
// MIDDLEWARE SIRALAMASI (BURASI ÇOK ÖNEMLÝ)
// ----------------------------------------------------------------

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// ÖNCE: Kimlik Doðrulama (Kimsin?)
app.UseAuthentication();

// SONRA: Yetkilendirme (Girebilir misin?)
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
// ... yukarýdaki kodlar ayný kalsýn ...

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    // ESKÝSÝ: pattern: "{controller=Home}/{action=Index}/{id?}");
    // YENÝSÝ (Aþaðýdaki gibi yap):
    pattern: "{controller=Account}/{action=Login}/{id?}");
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<RealEstateSite.Data.ApplicationDbContext>();
        RealEstateSite.Data.DbSeeder.Seed(context); // Oluþturduðun DbSeeder'ý çaðýrýyoruz
    }
    catch (Exception ex)
    {
        Console.WriteLine("Veri ekleme hatasý: " + ex.Message);
    }
}
app.Run();