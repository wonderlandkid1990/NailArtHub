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
    public class PartnerListModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly RegionService _regionService;

        public PartnerListModel(AppDbContext context, RegionService regionService)
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
        [BindProperty(SupportsGet = true)]
        public string SelectedCity { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SelectedDistrict { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? SelectedTagId { get; set; }

        public List<Shop> DisplayShops { get; set; }
        public List<NailTag> AllTags { get; set; }

        public async Task OnGetAsync()
        {
            AllTags = await _context.NailTags.OrderBy(t => t.TagName).ToListAsync();

            var shopQuery = _context.Shops
                .Include(s => s.ShopTagBridges)
                .ThenInclude(b => b.NailTag)
                .AsQueryable();

            if (SelectedTagId.HasValue)
            {
                shopQuery = shopQuery.Where(s => s.ShopTagBridges.Any(b => b.NailTagId == SelectedTagId.Value));
            }

            var regions = _regionService.GetTaiwanRegions();

            if (!string.IsNullOrEmpty(SelectedCity))
            {
                var targetCity = regions.FirstOrDefault(r => r.CityEn == SelectedCity);

                if (targetCity != null)
                {
                    var zhCity = targetCity.CityZh;

                    var alternativeZhCity = zhCity.Contains("台") ? zhCity.Replace("台", "臺") :
                                zhCity.Contains("臺") ? zhCity.Replace("臺", "台") : zhCity;

                    shopQuery = shopQuery.Where(s => s.Address.Contains(zhCity) || s.Address.Contains(alternativeZhCity));
                }
                else
                {
                    shopQuery = shopQuery.Where(s => s.Address.Contains(SelectedCity));
                }
            }

            if (!string.IsNullOrEmpty(SelectedDistrict))
            {
                var targetDistrict = regions
                    .SelectMany(r => r.Districts)
                    .FirstOrDefault(d => d.En == SelectedDistrict);

                if (targetDistrict != null)
                {
                    var zhDistrict = targetDistrict.Zh;
                    shopQuery = shopQuery.Where(s => s.Address.Contains(zhDistrict));
                }
                else
                {
                    shopQuery = shopQuery.Where(s => s.Address.Contains(SelectedDistrict));
                }
            }

            DisplayShops = await shopQuery.ToListAsync();
        }
    }
}