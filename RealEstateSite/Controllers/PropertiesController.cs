using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RealEstateSite.Data;
using RealEstateSite.Models;

namespace RealEstateSite.Controllers
{
    public class PropertiesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;

        public PropertiesController(ApplicationDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }

        // --- 1. INDEX: DÜZELTİLMİŞ HALİ ---
        public async Task<IActionResult> Index(
            string search,
            string status,   // Home'dan gelen "Sale" veya "Rent"
            string category, // Home'dan gelen "Villa", "Apartment" vb.
            string[] cities,
            string[] districts,
            string neighborhood,
            int? minPrice, int? maxPrice,
            int? minSize, int? maxSize,
            string[] roomCounts,
            string[] heatingTypes,
            int[] selectedFeatures)
        {
            ViewBag.Features = await _context.Features.ToListAsync();

            var query = _context.Properties
                .Include(p => p.Agent)
                .Include(p => p.PropertyFeatures)
                .AsQueryable();

            // 1. Arama Kutusu
            if (!string.IsNullOrEmpty(search))
            {
                string searchKey = search.Trim().ToUpper();
                query = query.Where(p =>
                    p.Title.ToUpper().Contains(searchKey) ||
                    p.City.ToUpper().Contains(searchKey) ||
                    p.District.ToUpper().Contains(searchKey)
                );
            }

            // 2. STATUS (İsim çakışması düzeltildi: statusResult)
            if (!string.IsNullOrEmpty(status))
            {
                if (Enum.TryParse(typeof(ListingType), status, true, out var statusResult))
                {
                    query = query.Where(p => p.Type == (ListingType)statusResult);
                }
            }

            // 3. CATEGORY (İsim çakışması düzeltildi: categoryResult)
            if (!string.IsNullOrEmpty(category) && category != "All Types")
            {
                if (Enum.TryParse(typeof(PropertyCategory), category, true, out var categoryResult))
                {
                    query = query.Where(p => p.Category == (PropertyCategory)categoryResult);
                }
            }

            // 4. Şehir
            if (cities != null && cities.Length > 0)
            {
                var cleanCities = cities.Where(c => !string.IsNullOrEmpty(c)).ToList();
                if (cleanCities.Any()) query = query.Where(p => cleanCities.Contains(p.City));
            }

            // 5. İlçe
            if (districts != null && districts.Length > 0)
            {
                var cleanDistricts = districts.Where(d => !string.IsNullOrEmpty(d)).ToList();
                if (cleanDistricts.Any()) query = query.Where(p => cleanDistricts.Contains(p.District));
            }

            // 6. Mahalle
            if (!string.IsNullOrEmpty(neighborhood))
            {
                string nb = neighborhood.Trim().ToLower();
                query = query.Where(p => p.Neighborhood != null && p.Neighborhood.ToLower().Contains(nb));
            }

            // 7. Oda & Isıtma
            if (roomCounts != null && roomCounts.Length > 0)
            {
                var cleanRooms = roomCounts.Where(r => !string.IsNullOrEmpty(r)).ToList();
                if (cleanRooms.Any()) query = query.Where(p => cleanRooms.Contains(p.RoomCount));
            }

            if (heatingTypes != null && heatingTypes.Length > 0)
                query = query.Where(p => heatingTypes.Contains(p.Heating.ToString()));

            // 8. Fiyat ve m2
            if (minPrice.HasValue) query = query.Where(p => p.Price >= minPrice.Value);
            if (maxPrice.HasValue) query = query.Where(p => p.Price <= maxPrice.Value);
            if (minSize.HasValue) query = query.Where(p => p.SquareMeters >= minSize.Value);
            if (maxSize.HasValue) query = query.Where(p => p.SquareMeters <= maxSize.Value);

            // 9. Özellikler
            if (selectedFeatures != null && selectedFeatures.Length > 0)
            {
                foreach (var fid in selectedFeatures)
                {
                    query = query.Where(p => p.PropertyFeatures.Any(pf => pf.FeatureId == fid));
                }
            }

            // Sonuç listesi (Buradaki 'result' ismi artık diğerleriyle çakışmaz)
            var result = await query.OrderByDescending(p => p.ListingDate).ToListAsync();

            return View(result);
        }

        // --- 2. DETAILS ---
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var property = await _context.Properties
                .Include(p => p.Agent)
                .Include(p => p.Photos)
                .Include(p => p.PropertyFeatures).ThenInclude(pf => pf.Feature)
                .FirstOrDefaultAsync(m => m.Id == id);
            return property == null ? NotFound() : View(property);
        }

        // --- 3. CREATE ---
        public IActionResult Create()
        {
            ViewBag.Features = _context.Features.ToList();
            ViewData["AgentId"] = new SelectList(_context.Agents, "Id", "FirstName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Property property, List<IFormFile> imageFiles, int[] selectedFeatureIds)
        {
            if (ModelState.IsValid)
            {
                property.Photos = new List<Photo>();
                string uploadPath = Path.Combine(_hostEnvironment.WebRootPath, "images/properties");
                if (!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath);

                if (imageFiles != null && imageFiles.Count > 0)
                {
                    foreach (var file in imageFiles)
                    {
                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                        using (var stream = new FileStream(Path.Combine(uploadPath, fileName), FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        var relativePath = "/images/properties/" + fileName;
                        property.Photos.Add(new Photo { Url = relativePath });

                        if (string.IsNullOrEmpty(property.ImageUrl))
                            property.ImageUrl = relativePath;
                    }
                }

                if (selectedFeatureIds != null)
                {
                    property.PropertyFeatures = selectedFeatureIds
                        .Select(id => new PropertyFeature { FeatureId = id }).ToList();
                }

                _context.Add(property);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Features = _context.Features.ToList();
            ViewData["AgentId"] = new SelectList(_context.Agents, "Id", "FirstName", property.AgentId);
            return View(property);
        }

        // --- 4. EDIT ---
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var property = await _context.Properties
                .Include(p => p.PropertyFeatures)
                .Include(p => p.Photos)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (property == null) return NotFound();

            ViewBag.Features = _context.Features.ToList();
            ViewBag.SelectedFeatures = property.PropertyFeatures.Select(pf => pf.FeatureId).ToArray();
            ViewData["AgentId"] = new SelectList(_context.Agents, "Id", "FirstName", property.AgentId);
            return View(property);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Property property, List<IFormFile> imageFiles, int[] selectedFeatureIds)
        {
            if (id != property.Id) return NotFound();

            ModelState.Remove("ImageUrl");
            ModelState.Remove("Agent");

            if (ModelState.IsValid)
            {
                try
                {
                    var existingProperty = await _context.Properties
                                            .Include(p => p.PropertyFeatures)
                                            .FirstOrDefaultAsync(p => p.Id == id);

                    if (existingProperty == null) return NotFound();

                    existingProperty.Title = property.Title;
                    existingProperty.Description = property.Description;
                    existingProperty.Price = property.Price;
                    existingProperty.SquareMeters = property.SquareMeters;
                    existingProperty.RoomCount = property.RoomCount;
                    existingProperty.BuildingAge = property.BuildingAge;
                    existingProperty.Heating = property.Heating;
                    existingProperty.City = property.City;
                    existingProperty.District = property.District;
                    existingProperty.Neighborhood = property.Neighborhood;
                    existingProperty.Address = property.Address;
                    existingProperty.Type = property.Type;
                    existingProperty.Category = property.Category;
                    existingProperty.AgentId = property.AgentId;
                    existingProperty.IsActive = property.IsActive;

                    if (imageFiles != null && imageFiles.Count > 0)
                    {
                        string uploadPath = Path.Combine(_hostEnvironment.WebRootPath, "images/properties");
                        foreach (var file in imageFiles)
                        {
                            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                            using (var stream = new FileStream(Path.Combine(uploadPath, fileName), FileMode.Create))
                            {
                                await file.CopyToAsync(stream);
                            }
                            _context.Photos.Add(new Photo { PropertyId = id, Url = "/images/properties/" + fileName });

                            if (string.IsNullOrEmpty(existingProperty.ImageUrl))
                                existingProperty.ImageUrl = "/images/properties/" + fileName;
                        }
                    }

                    _context.PropertyFeatures.RemoveRange(existingProperty.PropertyFeatures);
                    if (selectedFeatureIds != null)
                    {
                        foreach (var fid in selectedFeatureIds)
                        {
                            _context.PropertyFeatures.Add(new PropertyFeature { PropertyId = id, FeatureId = fid });
                        }
                    }

                    _context.Update(existingProperty);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PropertyExists(property.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Features = _context.Features.ToList();
            ViewData["AgentId"] = new SelectList(_context.Agents, "Id", "FirstName", property.AgentId);
            return View(property);
        }

        // --- 5. DELETE ---
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var property = await _context.Properties.Include(p => p.Agent).FirstOrDefaultAsync(m => m.Id == id);
            return property == null ? NotFound() : View(property);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var property = await _context.Properties
                .Include(p => p.Photos)
                .Include(p => p.PropertyFeatures)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (property != null)
            {
                foreach (var photo in property.Photos)
                {
                    var path = Path.Combine(_hostEnvironment.WebRootPath, photo.Url.TrimStart('/'));
                    if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
                }

                _context.Photos.RemoveRange(property.Photos);
                _context.PropertyFeatures.RemoveRange(property.PropertyFeatures);
                _context.Properties.Remove(property);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> DeletePhoto(int id)
        {
            var photo = await _context.Photos.FindAsync(id);
            if (photo == null) return Json(new { success = false });
            var path = Path.Combine(_hostEnvironment.WebRootPath, photo.Url.TrimStart('/'));
            if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
            _context.Photos.Remove(photo);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        private bool PropertyExists(int id) => _context.Properties.Any(e => e.Id == id);
    }
}