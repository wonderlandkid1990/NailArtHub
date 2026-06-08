using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;

namespace NailArtHub.Services
{
    public class DistrictModel
    {
        public string En { get; set; }
        public string Zh { get; set; }
    }

    public class RegionModel
    {
        public string CityEn { get; set; }
        public string CityZh { get; set; }
        public List<DistrictModel> Districts { get; set; }
    }

    public class RegionService
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        public RegionService(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        public List<RegionModel> GetTaiwanRegions()
        {
            var jsonPath = Path.Combine(_webHostEnvironment.WebRootPath, "data", "taiwan-regions.json");
            if (!File.Exists(jsonPath)) return new List<RegionModel>();

            var jsonData = File.ReadAllText(jsonPath);
            return JsonSerializer.Deserialize<List<RegionModel>>(jsonData, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
    }
}