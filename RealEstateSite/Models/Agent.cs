using System.Collections.Generic; // List ve ICollection için bu kütüphane şart
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RealEstateSite.Models
{
    public class Agent
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "First Name is required")]
        [Display(Name = "First Name")]
        public string? FirstName { get; set; }

        [Required(ErrorMessage = "Last Name is required")]
        [Display(Name = "Last Name")]
        public string? LastName { get; set; }

        [Required(ErrorMessage = "Title is required")]
        public string? Title { get; set; }

        // --- GÜNCELLENEN KISIM (TELEFON) ---
        [Required(ErrorMessage = "Phone Number is required")]
        [Display(Name = "Phone Number")]
        // Regex: 0 veya 5 ile başlayan, toplam 10-11 haneli numara kontrolü
        [RegularExpression(@"^(0?5\d{9})$", ErrorMessage = "Please enter a valid phone number (e.g. 05551234567)")]
        public string? PhoneNumber { get; set; }
        // -----------------------------------

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string? Email { get; set; }

        public string? ProfileImageUrl { get; set; }

        // Formda dosya yükleme (IFormFile) için gerekli alan
        [NotMapped]
        [Display(Name = "Upload Photo")]
        public Microsoft.AspNetCore.Http.IFormFile? ImageFile { get; set; }

        [Required(ErrorMessage = "Biography is required")]
        public string? Biography { get; set; }

        // --- GÜNCELLENEN KISIM (ŞİFRE) ---
        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        // En az 8 karakter, 1 Büyük Harf, 1 Küçük Harf, 1 Rakam, 1 Sembol
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$",
            ErrorMessage = "Password must be at least 8 characters and contain at least 1 uppercase letter, 1 number, and 1 symbol.")]
        public string? Password { get; set; }
        // ---------------------------------

        public string FullName => $"{FirstName} {LastName}";

        public bool Status { get; set; } = true;

        // *** EKLENEN KISIM: Controller hatasını çözen satır ***
        // Bir emlakçının birden fazla ilanı olabilir.
        public virtual ICollection<Property>? Properties { get; set; }
    }
}