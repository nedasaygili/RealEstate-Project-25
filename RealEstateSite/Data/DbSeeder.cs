using Microsoft.EntityFrameworkCore;
using RealEstateSite.Models;
using System.Collections.Generic;
using System.Linq;

namespace RealEstateSite.Data
{
    public static class DbSeeder
    {
        public static void Seed(ApplicationDbContext context)
        {
            // Veritabanı yoksa oluşturur
            context.Database.EnsureCreated();

            // SADECE ÖZELLİKLERİ (FEATURES) EKLER
            // Eğer veritabanında hiç özellik yoksa çalışır
            if (!context.Features.Any())
            {
                var features = new List<Feature>
                {
                    new Feature { Name = "Elevator" },          // Asansör
                    new Feature { Name = "Steel Door" },        // Çelik Kapı
                    new Feature { Name = "Parking Garage" },    // Kapalı Otopark
                    new Feature { Name = "Open Parking" },      // Açık Otopark
                    new Feature { Name = "Swimming Pool" },     // Yüzme Havuzu
                    new Feature { Name = "Gym" },               // Spor Salonu
                    new Feature { Name = "Security" },          // Güvenlik
                    new Feature { Name = "Air Conditioning" },  // Klima
                    new Feature { Name = "Balcony" },           // Balkon
                    new Feature { Name = "Ensuite Bathroom" },  // Ebeveyn Banyosu
                    new Feature { Name = "Fiber Internet" },    // Fiber İnternet
                    new Feature { Name = "Natural Gas" },       // Doğalgaz
                    new Feature { Name = "Underfloor Heating" },// Yerden Isıtma
                    new Feature { Name = "Terrace" },           // Teras
                    new Feature { Name = "Garden" }             // Bahçeli
                };

                context.Features.AddRange(features);
                context.SaveChanges();
            }
        }
    }
}