using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RealEstateSite.Models
{
    // 1. Şehir (İl)
    public class City
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public List<County> Counties { get; set; } = new List<County>();
    }

    // 2. İlçe
    public class County
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public int CityId { get; set; }
        public City City { get; set; }
        public List<DistrictDetail> Districts { get; set; } = new List<DistrictDetail>();
    }

    // 3. Semt Grubu
    public class DistrictDetail
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public int CountyId { get; set; }
        public County County { get; set; }
        public List<Neighborhood> Neighborhoods { get; set; } = new List<Neighborhood>();
    }

    // 4. Mahalle
    public class Neighborhood
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public int DistrictDetailId { get; set; }
        public DistrictDetail DistrictDetail { get; set; }
    }

    // 5. Açık Adres Modeli (İlanlarda kullanılacak)
    public class Address
    {
        [Key]
        public int Id { get; set; }
        public string City { get; set; }
        public string District { get; set; }
        public string Neighborhood { get; set; }
        public string Street { get; set; }
        public string? BuildingNo { get; set; }
        public int? Floor { get; set; }
        public string? DoorNo { get; set; }
        public string? FullAddress { get; set; }
    }
}