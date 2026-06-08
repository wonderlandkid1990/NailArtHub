using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NailArtHub.Data;
using NailArtHub.Models;
using NailArtHub.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NailArtHub.Pages
{
    public class ServicesModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly RegionService _regionService;

        public ServicesModel(AppDbContext context, RegionService regionService)
        {
            _context = context;
            _regionService = regionService;
        }
        public IActionResult OnPostSetLanguage(string culture, string returnUrl)
        {
            if (!string.IsNullOrEmpty(culture))
            {
                Response.Cookies.Append(
                    Microsoft.AspNetCore.Localization.CookieRequestCultureProvider.DefaultCookieName,
                    Microsoft.AspNetCore.Localization.CookieRequestCultureProvider.MakeCookieValue(new Microsoft.AspNetCore.Localization.RequestCulture(culture)),
                    new Microsoft.AspNetCore.Http.CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1), Path = "/" }
                );
            }

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }
            return RedirectToPage("/Index");
        }
        public IList<NailTrend> TrendResults { get; set; } = new List<NailTrend>();
        public IList<NailTag> AvailableTags { get; set; } = new List<NailTag>();
        public List<Shop> FeaturedShops { get; set; } = new List<Shop>();

        [BindProperty(SupportsGet = true)]
        public string SearchQuery { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? SelectedTagId { get; set; }
        public List<NailTag> AllTags { get; set; } = new List<NailTag>();

        [BindProperty(SupportsGet = true)]
        public List<string> SelectedTags { get; set; } = new List<string>();

        public string CurrentDisplayTag { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SelectedCity { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SelectedDistrict { get; set; }

        public async Task OnGetAsync()
        {
            AllTags = await _context.NailTags.ToListAsync();

            AvailableTags = await _context.NailTags
                .OrderByDescending(t => t.ViewCount)
                .Take(6)
                .ToListAsync();

            var query = _context.NailTrends.AsQueryable();

            if (SelectedTagId.HasValue)
            {
                var currentTag = AllTags.FirstOrDefault(t => t.Id == SelectedTagId.Value);
                if (currentTag != null)
                {
                    string cleaned = currentTag.TagName.ToLower().Replace(" ", "").Replace("#", "");
                    query = query.Where(t => t.Tag.ToLower().Trim() == cleaned);
                    CurrentDisplayTag = currentTag.TagName.ToUpper();
                }
            }
            else if (!string.IsNullOrEmpty(SearchQuery))
            {
                string cleanedSearchLower = SearchQuery.ToLower().Replace(" ", "").Replace("#", "");
                query = query.Where(t => t.Title.ToLower().Contains(cleanedSearchLower) || t.Tag.ToLower().Trim().Contains(cleanedSearchLower));
            }

            TrendResults = await query.OrderByDescending(t => t.Id).ToListAsync();

            if (string.IsNullOrEmpty(CurrentDisplayTag) && TrendResults.Any())
            {
                CurrentDisplayTag = TrendResults.First().Tag.ToUpper();
            }

            var shopQuery = _context.Shops
                .Include(s => s.ShopTagBridges)
                    .ThenInclude(b => b.NailTag)
                .AsQueryable();

            var regions = _regionService.GetTaiwanRegions();

            if (!string.IsNullOrEmpty(SelectedCity))
            {
                string cleanInputCity = SelectedCity.ToLower().Replace("city", "").Trim();
                var targetCity = regions.FirstOrDefault(r => r.CityEn.ToLower().Contains(cleanInputCity) || r.CityZh.Contains(cleanInputCity));

                if (targetCity != null)
                {
                    var zhCity = targetCity.CityZh;
                    var alternativeZhCity = zhCity.Contains("台") ? zhCity.Replace("台", "臺") :
                                            zhCity.Contains("臺") ? zhCity.Replace("臺", "台") : zhCity;

                    shopQuery = shopQuery.Where(s => s.Address != null && (s.Address.Contains(zhCity) || s.Address.Contains(alternativeZhCity)));
                }
                else
                {
                    shopQuery = shopQuery.Where(s => s.Address != null && s.Address.Contains(SelectedCity));
                }
            }

            if (!string.IsNullOrEmpty(SelectedDistrict))
            {
                string cleanInputDist = SelectedDistrict.ToLower().Replace("district", "").Trim();
                var targetDistrict = regions.SelectMany(r => r.Districts)
                    .FirstOrDefault(d => d.En.ToLower().Contains(cleanInputDist) || d.Zh.Contains(cleanInputDist));

                if (targetDistrict != null)
                {
                    shopQuery = shopQuery.Where(s => s.Address != null && s.Address.Contains(targetDistrict.Zh));
                }
            }

            if (SelectedTagId.HasValue)
            {
                var currentTag = AllTags.FirstOrDefault(t => t.Id == SelectedTagId.Value);
                if (currentTag != null)
                {
                    string targetTagNameLower = currentTag.TagName.ToLower().Replace(" ", "").Replace("#", "");
                    shopQuery = shopQuery.Where(s => s.ShopTagBridges.Any(b => b.NailTag != null &&
                        b.NailTag.TagName.ToLower().Replace(" ", "").Replace("#", "") == targetTagNameLower));
                }
            }

            FeaturedShops = await shopQuery.OrderByDescending(s => s.Id).Take(5).ToListAsync();

            ViewData["SelectedCity"] = SelectedCity;
            ViewData["SelectedDistrict"] = SelectedDistrict;
        }
    }
}