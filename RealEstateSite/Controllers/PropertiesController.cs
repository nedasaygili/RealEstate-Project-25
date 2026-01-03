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
using System.Globalization;
using System.Text;

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

        private string GetCanonicalName(string text)
        {
            if (string.IsNullOrEmpty(text)) return "";
            if (text.Contains("(")) text = text.Split('(')[0];
            string s = text.Trim().ToUpper(new CultureInfo("tr-TR"));
            s = s.Replace("İ", "I").Replace("ı", "I").Replace("Ğ", "G").Replace("ğ", "G").Replace("Ü", "U").Replace("ü", "U").Replace("Ş", "S").Replace("ş", "S").Replace("Ö", "O").Replace("ö", "O").Replace("Ç", "C").Replace("ç", "C");
            var sb = new StringBuilder();
            foreach (char c in s) { if (char.IsLetterOrDigit(c)) sb.Append(c); }
            return sb.ToString();
        }

        public async Task<IActionResult> Index(
            string search, string status, string category,
            string[] cities, string[] districts,
            int? minPrice, int? maxPrice, int? minSize, int? maxSize,
            string[] roomCounts, int[] heatingTypes, int[] selectedFeatures,
            string listingStatus)
        {
            ViewBag.Features = await _context.Features
                .Where(f => f.Name != "Underfloor Heating" && f.Name != "Natural Gas")
                .ToListAsync();

            ViewBag.CurrentListingStatus = listingStatus;

            var query = _context.Properties
                .Include(p => p.Agent)
                .Include(p => p.PropertyFeatures)
                .AsQueryable();

            if (string.IsNullOrEmpty(listingStatus) || listingStatus == "Active") query = query.Where(p => p.IsActive == true);
            else if (listingStatus == "Passive") query = query.Where(p => p.IsActive == false);

            if (!string.IsNullOrEmpty(search))
            {
                string s = GetCanonicalName(search);
                query = query.Where(p =>
                    GetCanonicalName(p.Title).Contains(s) ||
                    GetCanonicalName(p.City).Contains(s) ||
                    GetCanonicalName(p.District).Contains(s)
                );
            }

            if (!string.IsNullOrEmpty(status) && Enum.TryParse(typeof(ListingType), status, true, out var sr)) query = query.Where(p => p.Type == (ListingType)sr);
            if (!string.IsNullOrEmpty(category) && category != "All Types" && Enum.TryParse(typeof(PropertyCategory), category, true, out var cr)) query = query.Where(p => p.Category == (PropertyCategory)cr);

            if (minPrice.HasValue) query = query.Where(p => p.Price >= minPrice.Value);
            if (maxPrice.HasValue) query = query.Where(p => p.Price <= maxPrice.Value);
            if (minSize.HasValue) query = query.Where(p => p.SquareMeters >= minSize.Value);
            if (maxSize.HasValue) query = query.Where(p => p.SquareMeters <= maxSize.Value);

            // DÜZELTME: vr hatası giderildi -> validRooms
            if (roomCounts != null && roomCounts.Any(r => !string.IsNullOrEmpty(r)))
            {
                var validRooms = roomCounts.Where(r => !string.IsNullOrEmpty(r)).ToList();
                if (validRooms.Count > 0)
                {
                    query = query.Where(p => validRooms.Contains(p.RoomCount));
                }
            }

            // DÜZELTME: Nullable Heating kontrolü
            if (heatingTypes != null && heatingTypes.Length > 0)
            {
                var selectedHeatings = heatingTypes.Select(h => (HeatingType)h).ToList();
                query = query.Where(p => p.Heating.HasValue && selectedHeatings.Contains(p.Heating.Value));
            }

            if (selectedFeatures != null && selectedFeatures.Length > 0)
            {
                foreach (var fid in selectedFeatures)
                    query = query.Where(p => p.PropertyFeatures.Any(pf => pf.FeatureId == fid));
            }

            var resultList = await query.OrderByDescending(p => p.ListingDate).ToListAsync();

            if (cities != null && cities.Any(c => !string.IsNullOrEmpty(c)))
            {
                var targetCities = cities.Where(c => !string.IsNullOrEmpty(c)).Select(c => GetCanonicalName(c)).ToList();
                if (targetCities.Count > 0)
                {
                    resultList = resultList.Where(p => p.City != null && targetCities.Contains(GetCanonicalName(p.City))).ToList();
                }
            }

            if (districts != null && districts.Any(d => !string.IsNullOrEmpty(d)))
            {
                var targetDistricts = districts.Where(d => !string.IsNullOrEmpty(d)).Select(d => GetCanonicalName(d)).ToList();
                if (targetDistricts.Count > 0)
                {
                    resultList = resultList.Where(p => p.District != null && targetDistricts.Contains(GetCanonicalName(p.District))).ToList();
                }
            }

            return View(resultList);
        }

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

        public IActionResult Create()
        {
            ViewBag.Features = _context.Features
                .Where(f => f.Name != "Underfloor Heating" && f.Name != "Natural Gas")
                .ToList();
            ViewData["AgentId"] = new SelectList(_context.Agents, "Id", "FirstName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Property property, List<IFormFile> imageFiles, int[] selectedFeatureIds)
        {
            // Arsa ise nullable alanları null yap
            if (property.Category == PropertyCategory.Land)
            {
                ModelState.Remove("RoomCount");
                ModelState.Remove("Heating");
                ModelState.Remove("BuildingAge");
                property.RoomCount = null;
                property.BuildingAge = null;
                property.Heating = null;
            }

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
                        if (string.IsNullOrEmpty(property.ImageUrl)) property.ImageUrl = relativePath;
                    }
                }

                if (selectedFeatureIds != null)
                {
                    property.PropertyFeatures = selectedFeatureIds.Select(id => new PropertyFeature { FeatureId = id }).ToList();
                }

                _context.Add(property);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Features = _context.Features
                .Where(f => f.Name != "Underfloor Heating" && f.Name != "Natural Gas")
                .ToList();
            ViewData["AgentId"] = new SelectList(_context.Agents, "Id", "FirstName", property.AgentId);
            return View(property);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var property = await _context.Properties
                .Include(p => p.PropertyFeatures)
                .Include(p => p.Photos)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (property == null) return NotFound();

            ViewBag.Features = _context.Features
                .Where(f => f.Name != "Underfloor Heating" && f.Name != "Natural Gas")
                .ToList();
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

            if (property.Category == PropertyCategory.Land)
            {
                ModelState.Remove("RoomCount");
                ModelState.Remove("Heating");
                ModelState.Remove("BuildingAge");
                property.RoomCount = null;
                property.BuildingAge = null;
                property.Heating = null;
            }

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
                            if (string.IsNullOrEmpty(existingProperty.ImageUrl)) existingProperty.ImageUrl = "/images/properties/" + fileName;
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

            ViewBag.Features = _context.Features
                .Where(f => f.Name != "Underfloor Heating" && f.Name != "Natural Gas")
                .ToList();
            ViewData["AgentId"] = new SelectList(_context.Agents, "Id", "FirstName", property.AgentId);
            return View(property);
        }

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