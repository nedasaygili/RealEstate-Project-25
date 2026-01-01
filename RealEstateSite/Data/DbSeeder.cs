using RealEstateSite.Models;

namespace RealEstateSite.Data
{
    public static class DbSeeder
    {
        public static void Seed(ApplicationDbContext context)
        {
            // 1. Eğer Veritabanı yoksa oluştur (Garanti olsun)
            context.Database.EnsureCreated();

            // 2. ÖZELLİKLERİ EKLE (Elevator, Wifi vb.)
            // Eğer Features tablosu boşsa ekleme yap
            if (!context.Features.Any())
            {
                var features = new List<Feature>
                {
                    new Feature { Name = "Elevator" },
                    new Feature { Name = "Open Parking" },
                    new Feature { Name = "Security" },
                    new Feature { Name = "Ensuite Bathroom" },
                    new Feature { Name = "Underfloor Heating" },
                    new Feature { Name = "Steel Door" },
                    new Feature { Name = "Swimming Pool" },
                    new Feature { Name = "Air Conditioning" },
                    new Feature { Name = "Fiber Internet" },
                    new Feature { Name = "Terrace" },
                    new Feature { Name = "Parking Garage" },
                    new Feature { Name = "Gym" },
                    new Feature { Name = "Balcony" },
                    new Feature { Name = "Natural Gas" },
                    new Feature { Name = "Garden" }
                };

                context.Features.AddRange(features);
                context.SaveChanges(); // Veritabanına kaydet
            }

            // Buraya istersen Agent (Admin) ekleme kodunu da koyabilirsin, 
            // ama şu an acil olan Features kısmıydı.
        }
    }
}