namespace NailArtHub.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    namespace NailArtHub.Models
    {
        public class NewApply
        {
            public int Id { get; set; }

            [Required(ErrorMessage = "Please enter the shop name")]
            public string ShopName { get; set; }

            [Required(ErrorMessage = "Please enter your name")]
            public string OwnerName { get; set; }

            [Required(ErrorMessage = "Please enter the address")]
            public string Address { get; set; }

            [Required(ErrorMessage = "Please select the location")]
            public string Location { get; set; }

            public string InstagramUrl { get; set; }
            public string PinterestUrl { get; set; }

            public string SelectedTagsString { get; set; }

            public DateTime ApplyDate { get; set; } = DateTime.Now;
            public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected
        }
    }
}
