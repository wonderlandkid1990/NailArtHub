using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NailArtHub.Models
{
    [Table("NailTrend")]
    public class NailTrend
    {
        [Key]
        public int Id { get; set; }

        public string Tag { get; set; }

        public string Title { get; set; }

        public string ImageUrl { get; set; }

        public string SourceUrl { get; set; }

        public string CrawledAt { get; set; }
    }
}
