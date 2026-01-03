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
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(
            string status,   // "Sale", "Rent"
            string category, // "Apartment", "Villa"
            string city,
            string district,
            int? minPrice,
            int? maxPrice,
            string roomCount)
        {
            // Agent (Emlakçý) bilgisini dahil et
            var query = _context.Properties.Include(p => p.Agent).AsQueryable();
            bool isFiltering = false;

            // 1. Status (ListingType Enum)
            if (!string.IsNullOrEmpty(status))
            {
                if (Enum.TryParse(typeof(ListingType), status, true, out var result))
                {
                    query = query.Where(p => p.Type == (ListingType)result);
                    isFiltering = true;
                }
            }

            // 2. Category (PropertyCategory Enum)
            if (!string.IsNullOrEmpty(category) && category != "All Types")
            {
                if (Enum.TryParse(typeof(PropertyCategory), category, true, out var result))
                {
                    query = query.Where(p => p.Category == (PropertyCategory)result);
                    isFiltering = true;
                }
            }

            // 3. Location (City)
            if (!string.IsNullOrEmpty(city))
            {
                query = query.Where(p => p.City != null && p.City.ToLower() == city.ToLower());
                isFiltering = true;
            }

            // 4. Location (District)
            if (!string.IsNullOrEmpty(district))
            {
                query = query.Where(p => p.District != null && p.District.ToLower() == district.ToLower());
                isFiltering = true;
            }

            // 5. Price (Model decimal? olduðu için int? ile karþýlaþtýrýrken dikkat edilmeli)
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

            // Sadece Aktif Ýlanlar
            query = query.Where(p => p.IsActive == true);

            List<Property> resultList;

            if (isFiltering)
            {
                // Filtre varsa hepsini getir
                resultList = await query.OrderByDescending(p => p.ListingDate).ToListAsync();
            }
            else
            {
                // Filtre yoksa sadece Vitrin (Son 6 ilan)
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