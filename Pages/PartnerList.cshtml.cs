using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NailArtHub.Data;
using NailArtHub.Models;
using NailArtHub.Services;
using System;
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
            AllTags = await _context.NailTags
                                   .OrderByDescending(t => t.ViewCount)
                                   .Take(18)
                                   .ToListAsync();

            if (SelectedTagId.HasValue)
            {
                var clickedTag = AllTags.FirstOrDefault(t => t.Id == SelectedTagId.Value);
                if (clickedTag != null)
                {
                    clickedTag.ViewCount += 1;
                    _context.Entry(clickedTag).State = EntityState.Modified;
                    await _context.SaveChangesAsync();
                }
            }

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
                var zhCity = targetCity?.CityZh ?? SelectedCity;
                var alternativeZhCity = zhCity.Contains("台") ? zhCity.Replace("台", "臺") :
                                        zhCity.Contains("臺") ? zhCity.Replace("臺", "台") : zhCity;

                shopQuery = shopQuery.Where(s =>
                    s.City == zhCity ||
                    s.City == alternativeZhCity ||
                    s.Address.Contains(zhCity) ||
                    s.Address.Contains(alternativeZhCity)
                );
            }

            if (!string.IsNullOrEmpty(SelectedDistrict))
            {
                var targetDistrict = regions.SelectMany(r => r.Districts).FirstOrDefault(d => d.En == SelectedDistrict);
                var zhDistrict = targetDistrict?.Zh ?? SelectedDistrict;

                shopQuery = shopQuery.Where(s =>
                    s.District == zhDistrict ||
                    s.Address.Contains(zhDistrict)
                );
            }

            DisplayShops = await shopQuery.Distinct().ToListAsync();
        }
    }
}