using System.ComponentModel.DataAnnotations; // Kütüphane eklendi

namespace RealEstateSite.Models
{
    public class Admin
    {
        [Key] // <--- BU SATIR EKSİKTİ
        public int Id { get; set; }

        public string Username { get; set; }
        public string Password { get; set; }
    }
}