namespace NailArtHub.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    namespace NailArtHub.Models
    {
        public class NewApply
        {
            public int Id { get; set; }
            public string City { get; set; }
            public string District { get; set; }

            [Required(ErrorMessageResourceName = "Error_ShopName",
                      ErrorMessageResourceType = typeof(SharedResource))]
            public string ShopName { get; set; }

            [Required(ErrorMessageResourceName = "Error_OwnerName",
                      ErrorMessageResourceType = typeof(SharedResource))]
            public string OwnerName { get; set; }

            [Required(ErrorMessageResourceName = "Error_Address",
                      ErrorMessageResourceType = typeof(SharedResource))]
            public string Address { get; set; }

            [Required(ErrorMessageResourceName = "Error_Location",
                      ErrorMessageResourceType = typeof(SharedResource))]
            public string Location { get; set; }

            public string InstagramUrl { get; set; }
            public string? PinterestUrl { get; set; }

            [Required(ErrorMessageResourceName = "Error_TradeNo",
                      ErrorMessageResourceType = typeof(SharedResource))]
            public string PaymentProofCode { get; set; }
            public string? SelectedTagsString { get; set; }

            public DateTime ApplyDate { get; set; } = DateTime.Now;
            public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected
        }
    }
}
