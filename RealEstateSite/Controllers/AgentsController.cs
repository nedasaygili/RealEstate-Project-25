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
            // Sadece Aktif olanları mı görmek istersin yoksa hepsini mi?
            // Şimdilik hepsini getiriyoruz ama View tarafında pasifleri belli edebiliriz.
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

        // 3. EKLEME
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Agent agent, IFormFile? photoFile)
        {
            if (string.IsNullOrEmpty(agent.Password)) agent.Password = "123456";

            // Yeni eklenen her agent varsayılan olarak AKTİF olsun
            agent.Status = true;

            if (photoFile != null)
            {
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + photoFile.FileName;
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "img", "agents");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await photoFile.CopyToAsync(fileStream);
                }
                agent.ProfileImageUrl = "/img/agents/" + uniqueFileName;
            }
            else
            {
                agent.ProfileImageUrl = "https://via.placeholder.com/150";
            }

            if (ModelState.IsValid)
            {
                _context.Add(agent);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(agent);
        }

        // 4. DÜZENLEME
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var agent = await _context.Agents.FindAsync(id);
            if (agent == null) return NotFound();
            return View(agent);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Agent agent, IFormFile? photoFile)
        {
            if (id != agent.Id) return NotFound();

            var existingAgent = await _context.Agents.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);

            // Status bilgisini form göndermiyorsa, veritabanındaki eski halini koru
            // Veya Admin panelinde "Aktif/Pasif" checkbox'ı varsa onu kullan.
            // Şimdilik eski durumu koruyoruz:
            agent.Status = existingAgent.Status;

            if (ModelState.IsValid)
            {
                try
                {
                    if (photoFile != null)
                    {
                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + photoFile.FileName;
                        string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "img", "agents");
                        if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await photoFile.CopyToAsync(fileStream);
                        }
                        agent.ProfileImageUrl = "/img/agents/" + uniqueFileName;
                    }
                    else
                    {
                        agent.ProfileImageUrl = existingAgent.ProfileImageUrl;
                    }

                    if (string.IsNullOrEmpty(agent.Password))
                    {
                        agent.Password = existingAgent.Password;
                    }

                    _context.Update(agent);
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

        // 5. SİLME (DELETE) - GÜVENLİK GÜNCELLEMESİ
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            // GÜVENLİK: Admin silinemez
            if (id == 1)
            {
                return BadRequest("Ana Yönetici hesabı silinemez!");
            }

            var agent = await _context.Agents.FirstOrDefaultAsync(m => m.Id == id);
            if (agent == null) return NotFound();

            return View(agent);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // GÜVENLİK: Admin silinemez
            if (id == 1)
            {
                return BadRequest("Ana Yönetici hesabı silinemez!");
            }

            var agent = await _context.Agents.FindAsync(id);
            if (agent != null)
            {
                // DEĞİŞİKLİK: Veriyi tamamen silmiyoruz (Remove yok)
                // Sadece Status'u False yapıyoruz (Soft Delete)
                agent.Status = false;

                _context.Agents.Update(agent); // Update ile durumu kaydediyoruz
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool AgentExists(int id)
        {
            return _context.Agents.Any(e => e.Id == id);
        }

        // AgentsController.cs içine eklenecek:

        public async Task<IActionResult> ChangeStatus(int id)
        {
            // 1. Admin'i (ID: 1) korumaya al, ona kimse dokunamasın
            if (id == 1) return RedirectToAction(nameof(Index));

            // 2. Kişiyi bul
            var agent = await _context.Agents.FindAsync(id);
            if (agent == null) return NotFound();

            // 3. Durumu tersine çevir (True ise False, False ise True yap)
            agent.Status = !agent.Status;

            // 4. Kaydet ve Listeye dön
            _context.Update(agent);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

    }
}