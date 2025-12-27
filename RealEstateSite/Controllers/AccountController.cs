using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using RealEstateSite.Data;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace RealEstateSite.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            // Eğer kullanıcı zaten giriş yapmışsa Anasayfaya gönder
            if (User.Identity.IsAuthenticated) return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            // 1. BOŞLUK TEMİZLİĞİ
            email = email?.Trim();
            password = password?.Trim();

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Please fill in both email and password fields.";
                return View();
            }

            ClaimsIdentity identity = null;
            bool isAuthenticated = false;

            // ---------------------------------------------------------
            // 1. DURUM: SABİT ADMIN (PATRON) GİRİŞİ
            // ---------------------------------------------------------
            if (email == "admin@unea.com" && password == "Admin123!")
            {
                var claims = new List<Claim> {
                    new Claim(ClaimTypes.Name, "Admin"),
                    new Claim(ClaimTypes.Email, email),
                    new Claim(ClaimTypes.Role, "Admin")
                };
                identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                isAuthenticated = true;
            }
            // ---------------------------------------------------------
            // 2. DURUM: AGENT (DANIŞMAN) GİRİŞİ
            // ---------------------------------------------------------
            else
            {
                var agent = _context.Agents.FirstOrDefault(a => a.Email == email);

                if (agent != null)
                {
                    if (agent.Password != null && agent.Password.Trim() == password)
                    {
                        var claims = new List<Claim> {
                            // --- HATA DÜZELTİLDİ ---
                            // agent.FullName yerine Ad + Soyad birleştirildi
                            new Claim(ClaimTypes.Name, $"{agent.FirstName} {agent.LastName}"),
                            // -----------------------
                            
                            new Claim(ClaimTypes.Email, agent.Email),
                            new Claim(ClaimTypes.Role, "Agent"),
                            new Claim("AgentId", agent.Id.ToString())
                        };
                        identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                        isAuthenticated = true;
                    }
                    else
                    {
                        ViewBag.Error = "Invalid password. Please try again.";
                        return View();
                    }
                }
                else
                {
                    ViewBag.Error = "No agent found with this email address.";
                    return View();
                }
            }

            // ---------------------------------------------------------
            // GİRİŞ BAŞARILIYSA
            // ---------------------------------------------------------
            if (isAuthenticated && identity != null)
            {
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity), new AuthenticationProperties());
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Login failed. Please check your credentials.";
            return View();
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
    }
}