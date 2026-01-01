using RealEstateSite.Models; // Bu satır önemli

namespace RealEstateSite.Models
{
    public class AramaViewModel
    {
        public string? SearchTerm { get; set; }
        public PropertyCategory? Category { get; set; }
        public ListingType? Type { get; set; }
        public int? MinPrice { get; set; }
        public int? MaxPrice { get; set; }
        public string? RoomCount { get; set; }
    }
}