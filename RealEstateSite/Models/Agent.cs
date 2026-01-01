using System.ComponentModel.DataAnnotations;

namespace RealEstateSite.Models
{
    public class Agent
    {
        public int Id { get; set; }

        // string? yaparak "Veritabanından boş gelebilir, sorun çıkarma" diyoruz.
        // [Required] kuralı durduğu için formdan boş gönderilmesini hala engelliyoruz.

        [Required(ErrorMessage = "First Name is required")]
        public string? FirstName { get; set; }

        [Required(ErrorMessage = "Last Name is required")]
        public string? LastName { get; set; }

        [Required(ErrorMessage = "Title is required")]
        public string? Title { get; set; }

        [Required(ErrorMessage = "Phone Number is required")]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string? Email { get; set; }

        public string? ProfileImageUrl { get; set; } // Resim zaten zorunlu değildi

        [Required(ErrorMessage = "Biography is required")]
        public string? Biography { get; set; }

        [Required(ErrorMessage = "Password is required")]
        public string? Password { get; set; }

        // Bu özellik veritabanında yok, sadece kod içinde Ad+Soyad birleştirmek için kullanıyoruz
        // Eğer AccountController'da hala agent.FullName hatası alırsan bu satır hayat kurtarır:
        public string FullName => $"{FirstName} {LastName}";

        public bool Status { get; set; } = true;
    }

}