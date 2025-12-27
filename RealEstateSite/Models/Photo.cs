using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace RealEstateSite.Models
{
    public class Photo
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Url { get; set; } // Fotoğrafın dosya yolu (Örn: /images/properties/abc.jpg)

        // Hangi ilana ait olduğunu belirleyen Foreign Key
        public int PropertyId { get; set; }

        [ForeignKey("PropertyId")]
        [ValidateNever]
        public virtual Property? Property { get; set; }
    }
}