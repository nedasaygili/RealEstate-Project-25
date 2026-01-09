using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using RealEstateSite.Data;
using RealEstateSite.Models;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace RealEstateSite.Controllers
{
    [Authorize(Roles = "Admin, Agent")]
    public class AgentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public AgentsController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // 1. LİSTELEME
        public async Task<IActionResult> Index()
        {
            return View(await _context.Agents.ToListAsync());
        }

        // 2. DETAYLAR
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var agent = await _context.Agents.FirstOrDefaultAsync(m => m.Id == id);
            if (agent == null) return NotFound();
            return View(agent);
        }

        // 3. EKLEME (GET)
        public IActionResult Create()
        {
            return View();
        }

        // 3. EKLEME (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Agent agent, IFormFile? photoFile)
        {
            // A. Mail Kontrolü
            bool mailVarMi = await _context.Agents.AnyAsync(x => x.Email == agent.Email);
            if (mailVarMi)
            {
                ModelState.AddModelError("Email", "Bu e-posta adresi zaten sistemde kayıtlı!");
            }

            // B. Şifre Kontrolü
            if (string.IsNullOrEmpty(agent.Password))
            {
                ModelState.AddModelError("Password", "Lütfen bir şifre belirleyiniz.");
            }
            else if (!IsPasswordStrong(agent.Password))
            {
                ModelState.AddModelError("Password", "Şifre yeterince güçlü değil. (En az 8 karakter, 1 büyük harf, 1 sayı, 1 sembol)");
            }

            agent.Status = true;

            if (ModelState.IsValid)
            {
                if (photoFile != null)
                {
                    agent.ProfileImageUrl = await UploadFile(photoFile);
                }
                else
                {
                    agent.ProfileImageUrl = "https://via.placeholder.com/150";
                }

                _context.Add(agent);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(agent);
        }

        // 4. DÜZENLEME (GET)
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var agent = await _context.Agents.FindAsync(id);
            if (agent == null) return NotFound();

            agent.Password = ""; // Güvenlik
            return View(agent);
        }

        // 4. DÜZENLEME (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Agent agent, IFormFile? photoFile)
        {
            if (id != agent.Id) return NotFound();

            if (string.IsNullOrEmpty(agent.Password))
            {
                ModelState.Remove("Password");
            }

            bool emailIsTaken = await _context.Agents.AnyAsync(x => x.Email == agent.Email && x.Id != id);
            if (emailIsTaken)
            {
                ModelState.AddModelError("Email", "Bu e-posta adresi başka bir kullanıcı tarafından kullanılıyor.");
            }

            if (!string.IsNullOrEmpty(agent.Password) && !IsPasswordStrong(agent.Password))
            {
                ModelState.AddModelError("Password", "Yeni şifre kurallara uymuyor. (En az 8 karakter, 1 büyük harf, 1 sayı, 1 sembol)");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var agentInDb = await _context.Agents.FindAsync(id);
                    if (agentInDb == null) return NotFound();

                    agentInDb.FirstName = agent.FirstName;
                    agentInDb.LastName = agent.LastName;
                    agentInDb.Title = agent.Title;
                    agentInDb.PhoneNumber = agent.PhoneNumber;
                    agentInDb.Email = agent.Email;
                    agentInDb.Biography = agent.Biography;

                    if (!string.IsNullOrEmpty(agent.Password))
                    {
                        agentInDb.Password = agent.Password;
                    }

                    if (photoFile != null)
                    {
                        agentInDb.ProfileImageUrl = await UploadFile(photoFile);
                    }

                    _context.Update(agentInDb);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AgentExists(agent.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(agent);
        }

        // 5. SİLME (DELETE - GET)
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            if (id == 1) return BadRequest("Ana Yönetici hesabı silinemez!");

            var agent = await _context.Agents.FirstOrDefaultAsync(m => m.Id == id);
            if (agent == null) return NotFound();

            return View(agent);
        }

        // 5. SİLME (DELETE - POST) - **BURASI DÜZELTİLDİ**
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (id == 1) return BadRequest("Ana Yönetici hesabı silinemez!");

            // 1. Ajanı ve İlanlarını (Properties) getir
            var agent = await _context.Agents
                .Include(a => a.Properties)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (agent != null)
            {
                // 2. AŞAMA: ÖNCE İLANLARI SAHİPSİZ YAP VE KAYDET
                // Bu işlem, silme işleminden önce yapılmalı ki veritabanı bağları koparsın.
                if (agent.Properties != null && agent.Properties.Any())
                {
                    foreach (var property in agent.Properties)
                    {
                        property.AgentId = null; // İlanı boşa çıkar (Sisteme ata)
                    }

                    // DEĞİŞİKLİKLERİ HEMEN KAYDET
                    // Bu sayede ilanlar artık "AgentId: NULL" olarak veritabanına işlenir.
                    _context.UpdateRange(agent.Properties);
                    await _context.SaveChangesAsync();
                }

                // 3. AŞAMA: PROFİL RESMİNİ TEMİZLE
                if (!string.IsNullOrEmpty(agent.ProfileImageUrl) && !agent.ProfileImageUrl.StartsWith("http"))
                {
                    try
                    {
                        string webRootPath = _webHostEnvironment.WebRootPath;
                        string relativePath = agent.ProfileImageUrl.TrimStart('/');
                        string fullPath = Path.Combine(webRootPath, relativePath.Replace('/', Path.DirectorySeparatorChar));

                        if (System.IO.File.Exists(fullPath))
                        {
                            System.IO.File.Delete(fullPath);
                        }
                    }
                    catch
                    {
                        // Hata olursa devam et
                    }
                }

                // 4. AŞAMA: ARTIK AJANI GÜVENLE SİLEBİLİRSİN
                // İlanlar zaten boşa çıktığı için veritabanı onlara dokunmaz.
                _context.Agents.Remove(agent);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // YARDIMCI METODLAR

        private bool AgentExists(int id)
        {
            return _context.Agents.Any(e => e.Id == id);
        }

        public async Task<IActionResult> ChangeStatus(int id)
        {
            if (id == 1) return RedirectToAction(nameof(Index));

            var agent = await _context.Agents.FindAsync(id);
            if (agent == null) return NotFound();

            agent.Status = !agent.Status;
            _context.Update(agent);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private bool IsPasswordStrong(string password)
        {
            if (string.IsNullOrEmpty(password)) return false;
            if (password.Length < 8) return false;
            if (!Regex.IsMatch(password, "[A-Z]")) return false;
            if (!Regex.IsMatch(password, "[0-9]")) return false;
            if (!Regex.IsMatch(password, "[^a-zA-Z0-9]")) return false;
            return true;
        }

        private async Task<string> UploadFile(IFormFile photoFile)
        {
            string uniqueFileName = Guid.NewGuid().ToString() + "_" + photoFile.FileName;
            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "img", "agents");

            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            string filePath = Path.Combine(uploadsFolder, uniqueFileName);
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await photoFile.CopyToAsync(fileStream);
            }
            return "/img/agents/" + uniqueFileName;
        }
    }
}