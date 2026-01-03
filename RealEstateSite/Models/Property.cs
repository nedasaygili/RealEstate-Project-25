using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Http;

namespace RealEstateSite.Models
{
    public class Property
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Listing Title")]
        [Required(ErrorMessage = "Listing title is required.")]
        [StringLength(100, ErrorMessage = "Title cannot exceed 100 characters.")]
        public string Title { get; set; }

        [Display(Name = "Description")]
        [Required(ErrorMessage = "Description is required.")]
        public string Description { get; set; }

        [Display(Name = "Price")]
        [Required(ErrorMessage = "Price is required.")]
        [Range(0, double.MaxValue, ErrorMessage = "Price must be a positive number.")]
        [DisplayFormat(DataFormatString = "{0:N0} ₺")]
        public decimal? Price { get; set; } // Nullable yapıldı (Kritik)

        [Display(Name = "Cover Photo")]
        [ValidateNever]
        public string? ImageUrl { get; set; }

        // --- TECHNICAL SPECS (Hepsi Soru İşaretli - Nullable) ---

        [Display(Name = "Net Area (m²)")]
        [Required(ErrorMessage = "Square meters area is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please enter a valid area.")]
        public int? SquareMeters { get; set; }

        [Display(Name = "Room Count")]
        public string? RoomCount { get; set; }

        [Display(Name = "Building Age")]
        public int? BuildingAge { get; set; }

        [Display(Name = "Heating Type")]
        public HeatingType? Heating { get; set; }

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;

        // --- LOCATION INFO ---

        [Display(Name = "City")]
        [Required(ErrorMessage = "City is required.")]
        public string City { get; set; }

        [Display(Name = "District")]
        [Required(ErrorMessage = "District is required.")]
        public string District { get; set; }

        [Display(Name = "Neighborhood")]
        [Required(ErrorMessage = "Neighborhood is required.")]
        public string? Neighborhood { get; set; }

        [Display(Name = "Full Address")]
        [DataType(DataType.MultilineText)]
        public string? Address { get; set; }

        // --- CATEGORY & TYPE ---

        [Display(Name = "Listing Type")]
        [Required(ErrorMessage = "Listing type is required.")]
        public ListingType? Type { get; set; }

        [Display(Name = "Property Category")]
        [Required(ErrorMessage = "Category is required.")]
        public PropertyCategory? Category { get; set; }

        [Display(Name = "Listing Date")]
        [DataType(DataType.Date)]
        public DateTime ListingDate { get; set; } = DateTime.Now;

        // --- RELATIONS ---

        [Display(Name = "Real Estate Agent")]
        [Required(ErrorMessage = "Agent selection is required.")]
        public int? AgentId { get; set; }

        [ForeignKey("AgentId")]
        [ValidateNever]
        public Agent? Agent { get; set; }

        [ValidateNever]
        public List<Photo> Photos { get; set; } = new List<Photo>();

        [ValidateNever]
        public virtual ICollection<PropertyFeature> PropertyFeatures { get; set; } = new List<PropertyFeature>();
    }

    // --- ENUMLAR ---
    public enum ListingType { Sale, Rent }
    public enum PropertyCategory { Apartment, Villa, Office, Land }
    public enum HeatingType { None, Stove, NaturalGas, Central, Underfloor, AirConditioning }
}