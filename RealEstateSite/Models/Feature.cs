using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RealEstateSite.Models
{
    // Özellik Tanımı
    public class Feature
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }

        // İlişki
        public virtual ICollection<PropertyFeature> PropertyFeatures { get; set; }
    }

    // Ara Tablo (Çoka-Çok İlişki)
    public class PropertyFeature
    {
        public int PropertyId { get; set; }
        [ForeignKey("PropertyId")]
        public Property Property { get; set; }

        public int FeatureId { get; set; }
        [ForeignKey("FeatureId")]
        public Feature Feature { get; set; }
    }
}