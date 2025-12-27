using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealEstateSite.Data;
using RealEstateSite.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace RealEstateSite.Controllers
{
    // Authorize kaldýrdým, Home sayfasý genelde public'tir.
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(
            string keyword,
            string status,   // "Sale", "Rent" vb.
            string category, // "Apartment", "Villa" vb.
            string city,
            string district,
            string neighborhood, // Yeni alan
            int? minPrice,
            int? maxPrice,
            string roomCount)
        {
            var query = _context.Properties.Include(p => p.Agent).AsQueryable();
            bool isFiltering = false;

            // 1. Keyword (Title)
            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(p => p.Title.ToLower().Contains(keyword.ToLower()));
                isFiltering = true;
            }

            // 2. Status (ListingType Enum)
            if (!string.IsNullOrEmpty(status))
            {
                if (Enum.TryParse(typeof(ListingType), status, true, out var result))
                {
                    query = query.Where(p => p.Type == (ListingType)result);
                    isFiltering = true;
                }
            }

            // 3. Category (PropertyCategory Enum)
            if (!string.IsNullOrEmpty(category) && category != "All Types")
            {
                if (Enum.TryParse(typeof(PropertyCategory), category, true, out var result))
                {
                    query = query.Where(p => p.Category == (PropertyCategory)result);
                    isFiltering = true;
                }
            }

            // 4. Location
            if (!string.IsNullOrEmpty(city))
            {
                query = query.Where(p => p.City.ToLower() == city.ToLower());
                isFiltering = true;
            }

            if (!string.IsNullOrEmpty(district))
            {
                query = query.Where(p => p.District.ToLower() == district.ToLower());
                isFiltering = true;
            }

            // Neighborhood (Modeldeki sütuna göre filtreleme)
            if (!string.IsNullOrEmpty(neighborhood))
            {
                string nb = neighborhood.Trim().ToLower();
                query = query.Where(p => p.Neighborhood != null && p.Neighborhood.ToLower().Contains(nb));
                isFiltering = true;
            }

            // 5. Price
            if (minPrice.HasValue)
            {
                query = query.Where(p => p.Price >= minPrice.Value);
                isFiltering = true;
            }
            if (maxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= maxPrice.Value);
                isFiltering = true;
            }

            // 6. Room
            if (!string.IsNullOrEmpty(roomCount))
            {
                query = query.Where(p => p.RoomCount == roomCount);
                isFiltering = true;
            }

            // SONUÇLARI DÖNDÜR
            List<Property> resultList;

            if (isFiltering)
            {
                // Filtre varsa hepsini getir
                resultList = await query.OrderByDescending(p => p.ListingDate).ToListAsync();
            }
            else
            {
                // Filtre yoksa sadece Son 6 ilan (Vitrin)
                resultList = await query
                                    .OrderByDescending(p => p.ListingDate)
                                    .Take(6)
                                    .ToListAsync();
            }

            return View(resultList);
        }

        public IActionResult About() { return View(); }
        public IActionResult Services() { return View(); }
        public IActionResult Contact() { return View(); }
    }
}