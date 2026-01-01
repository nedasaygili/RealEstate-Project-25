using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RealEstateSite.Data;
using RealEstateSite.Models;
using System.IO;              // Dosya işlemleri için eklendi
using System.Text.Json;       // JSON işlemleri için eklendi

namespace RealEstateSite.Controllers
{
    public class AddressesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AddressesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // YENİ EKLENEN KISIM: JSON YÜKLEME
        // ==========================================

        // GET: Addresses/ImportLocation
        public IActionResult ImportLocation()
        {
            return View();
        }

        // POST: Addresses/ImportLocation
        [HttpPost]
        public async Task<IActionResult> ImportLocation(IFormFile jsonFile)
        {
            // 1. Dosya seçilmiş mi kontrol et
            if (jsonFile == null || jsonFile.Length == 0)
            {
                ViewBag.Error = "Lütfen geçerli bir JSON dosyası seçin.";
                return View();
            }

            try
            {
                // 2. Dosyayı akış olarak oku
                using (var stream = new StreamReader(jsonFile.OpenReadStream()))
                {
                    var jsonString = await stream.ReadToEndAsync();

                    // 3. JSON ayarları (Büyük/küçük harf duyarsızlığı)
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };

                    // 4. JSON'u City (İl) nesnesine çevir
                    // Senin JSON dosyan tek bir süslü parantez { } ile başladığı için tek nesneye çeviriyoruz.
                    var cityData = JsonSerializer.Deserialize<City>(jsonString, options);

                    if (cityData != null)
                    {
                        // 5. Veritabanına kaydet
                        // EF Core, City nesnesini kaydettiğinde altındaki County, District ve Neighborhood'ları da otomatik kaydeder.
                        _context.Cities.Add(cityData);

                        await _context.SaveChangesAsync();

                        ViewBag.Message = $"{cityData.Name} ili ve tüm alt birimleri başarıyla veritabanına aktarıldı.";
                    }
                    else
                    {
                        ViewBag.Error = "JSON dosyası okunamadı veya format hatalı.";
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Bir hata oluştu: {ex.Message}";
            }

            return View();
        }

        // ==========================================
        // MEVCUT CRUD İŞLEMLERİ (Senin Kodların)
        // ==========================================

        // GET: Addresses
        public async Task<IActionResult> Index()
        {
            return View(await _context.Addresses.ToListAsync());
        }

        // GET: Addresses/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var address = await _context.Addresses
                .FirstOrDefaultAsync(m => m.Id == id);
            if (address == null)
            {
                return NotFound();
            }

            return View(address);
        }

        // GET: Addresses/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Addresses/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,City,District,Neighborhood,Street,BuildingNo,Floor,DoorNo,FullAddress")] Address address)
        {
            if (ModelState.IsValid)
            {
                _context.Add(address);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(address);
        }

        // GET: Addresses/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var address = await _context.Addresses.FindAsync(id);
            if (address == null)
            {
                return NotFound();
            }
            return View(address);
        }

        // POST: Addresses/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,City,District,Neighborhood,Street,BuildingNo,Floor,DoorNo,FullAddress")] Address address)
        {
            if (id != address.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(address);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AddressExists(address.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(address);
        }

        // GET: Addresses/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var address = await _context.Addresses
                .FirstOrDefaultAsync(m => m.Id == id);
            if (address == null)
            {
                return NotFound();
            }

            return View(address);
        }

        // POST: Addresses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var address = await _context.Addresses.FindAsync(id);
            if (address != null)
            {
                _context.Addresses.Remove(address);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AddressExists(int id)
        {
            return _context.Addresses.Any(e => e.Id == id);
        }
    }
}