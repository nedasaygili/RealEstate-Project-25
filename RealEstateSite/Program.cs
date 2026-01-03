using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using RealEstateSite.Data;

var builder = WebApplication.CreateBuilder(args);

// --------------------------------------------------------
// 1. SERVÝSLERÝN EKLENMESÝ
// --------------------------------------------------------

// Veritabaný Baðlantýsý
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Authentication (Giriþ) Servisi
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login"; // Giriþ yapmamýþsa buraya at
        options.AccessDeniedPath = "/Account/Login"; // Yetkisi yetmiyorsa buraya at
    });

// Authorization (Yetki) Servisi
builder.Services.AddAuthorization();

// MVC Servisleri
builder.Services.AddControllersWithViews();

var app = builder.Build();

// --------------------------------------------------------
// 2. OTOMATÝK KURULUM VE VERÝ YÜKLEME (MIGRATION & SEEDING)
// --------------------------------------------------------
// Proje her baþladýðýnda veritabanýný kontrol eder.
// Yoksa oluþturur, varsa günceller ve eksik verileri tamamlar.

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<RealEstateSite.Data.ApplicationDbContext>();

        // --- ADIM A: OTOMATÝK MIGRATION ---
        // Bu satýr "Update-Database" komutunu senin yerine arka planda çalýþtýrýr.
        context.Database.Migrate();

        // --- ADIM B: VERÝ YÜKLEME (SEED) ---
        // DbSeeder içindeki Seed metodunu çalýþtýr (Admin, Emlakçý vb. ekle)
        RealEstateSite.Data.DbSeeder.Seed(context);
    }
    catch (Exception ex)
    {
        // Hata olursa konsola yaz (Loglama)
        Console.WriteLine("Veritabaný kurulumu sýrasýnda hata oluþtu: " + ex.Message);
    }
}

// --------------------------------------------------------
// 3. MIDDLEWARE SIRALAMASI
// --------------------------------------------------------

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

// ROTA AYARLAMASI
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// --------------------------------------------------------
// 4. UYGULAMAYI BAÞLAT
// --------------------------------------------------------

app.Run();