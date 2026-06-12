using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NailArtHub.Models
{
    public class Shop
    {
        public int Id { get; set; }
        public string? City { get; set; }
        public string? District { get; set; }

        [Required(ErrorMessage = "Please enter the shop name")]
        public string ShopName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter your name")]
        public string OwnerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter the address")]
        public string Address { get; set; } = string.Empty;

        // Location (Taipei/New Taipei) selector
        public string Location { get; set; } = string.Empty;

        public string InstagramUrl { get; set; } = string.Empty;
        public string PinterestUrl { get; set; } = string.Empty;

        // Agreement
        public bool IsAgreed { get; set; }

        public List<ShopTagBridge> ShopTagBridges { get; set; } = new List<ShopTagBridge>();
    }
}