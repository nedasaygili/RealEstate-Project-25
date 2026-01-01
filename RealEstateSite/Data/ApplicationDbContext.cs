using Microsoft.EntityFrameworkCore;
using RealEstateSite.Models;

namespace RealEstateSite.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // --- ANA TABLOLAR (MAIN TABLES) ---
        public DbSet<Property> Properties { get; set; }
        public DbSet<Agent> Agents { get; set; }
        public DbSet<Photo> Photos { get; set; }

        // --- ÖZELLİK TABLOLARI (FEATURE TABLES) ---
        public DbSet<Feature> Features { get; set; }
        public DbSet<PropertyFeature> PropertyFeatures { get; set; }

        // --- KONUM/ADRES TABLOLARI (LOCATION TABLES) ---
        public DbSet<Address> Addresses { get; set; }
        public DbSet<City> Cities { get; set; }
        public DbSet<County> Counties { get; set; }
        public DbSet<DistrictDetail> Districts { get; set; }
        public DbSet<Neighborhood> Neighborhoods { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Özellikler için Çoka-Çok İlişki Ayarı (Many-to-Many Configuration)
            modelBuilder.Entity<PropertyFeature>()
                .HasKey(pf => new { pf.PropertyId, pf.FeatureId });

            modelBuilder.Entity<PropertyFeature>()
                .HasOne(pf => pf.Property)
                .WithMany(p => p.PropertyFeatures)
                .HasForeignKey(pf => pf.PropertyId);

            modelBuilder.Entity<PropertyFeature>()
                .HasOne(pf => pf.Feature)
                .WithMany(f => f.PropertyFeatures)
                .HasForeignKey(pf => pf.FeatureId);

            modelBuilder.Entity<Property>()
                .Property(p => p.Price)
                .HasColumnType("decimal(18,2)");

            // --- SABİT ADMIN HESABI (SEED DATA) ---
            modelBuilder.Entity<Agent>().HasData(new Agent
            {
                Id = 1, // FIXED ID
                FirstName = "System", // Türkçe: Sistem -> İngilizce: System
                LastName = "Admin",   // Türkçe: Yöneticisi -> İngilizce: Admin
                Title = "Administrator",
                Email = "admin@unea.com",
                PhoneNumber = "05550000000",
                Password = "Admin123!",
                ProfileImageUrl = "https://via.placeholder.com/150",
                Status = true, // Admin is always active

                // HATA ÇÖZÜMÜ BURADA:
                Biography = "System Administrator Account." // Zorunlu alan eklendi (İngilizce)
            });
        }
    }
}