using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NailArtHub.Data;
using NailArtHub.Models;
using NailArtHub.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

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

        public IList<NailTrend> TrendResults { get; set; } = new List<NailTrend>();
        public IList<NailTag> AvailableTags { get; set; } = new List<NailTag>();
        public List<Shop> FeaturedShops { get; set; } = new List<Shop>();
        public List<NailTag> AllTags { get; set; } = new List<NailTag>();

        [BindProperty(SupportsGet = true)] public string SearchQuery { get; set; }
        [BindProperty(SupportsGet = true)] public int? SelectedTagId { get; set; }
        [BindProperty(SupportsGet = true)] public string SelectedCity { get; set; }
        [BindProperty(SupportsGet = true)] public string SelectedDistrict { get; set; }

        public string CurrentDisplayTag { get; set; }

        public async Task OnGetAsync()
        {
            AllTags = await _context.NailTags
                                   .OrderByDescending(t => t.ViewCount)
                                   .Take(10)
                                   .ToListAsync();
            IQueryable<NailTrend> query = _context.NailTrends;
            if (SelectedTagId.HasValue)
            {
                SearchQuery = null;

                var currentTag = AllTags.FirstOrDefault(t => t.Id == SelectedTagId.Value);
                if (currentTag != null)
                {
                    currentTag.ViewCount += 1;
                    _context.Entry(currentTag).State = EntityState.Modified;
                    await _context.SaveChangesAsync();

                    string cleaned = currentTag.TagName.ToLower().Trim().Replace(" ", "").Replace("#", "");
                    int trendCount = await _context.NailTrends.CountAsync(t => t.Tag.ToLower().Replace(" ", "").Replace("#", "") == cleaned);
                    if (trendCount < 3)
                    {
                        await RunPythonCrawlerAsync(cleaned);
                        AllTags = await _context.NailTags.OrderByDescending(t => t.ViewCount).Take(10).ToListAsync();
                    }
                    query = query.Where(t => t.Tag.ToLower().Trim().Replace(" ", "").Replace("#", "") == cleaned);
                    CurrentDisplayTag = currentTag.TagName.ToUpper();
                    AllTags = AllTags.OrderByDescending(t => t.ViewCount).ToList();
                }
            }

            else if(!string.IsNullOrEmpty(SearchQuery))
            {
                string q = SearchQuery.Trim().ToLower().Replace(" ", "").Replace("#", "");
                var existingTag = await _context.NailTags
                .FirstOrDefaultAsync(t => t.TagName.ToLower().Replace(" ", "").Replace("#", "") == q);
                int trendCount = await _context.NailTrends.CountAsync(t => t.Tag.ToLower().Replace(" ", "").Replace("#", "") == q);

                if (existingTag == null || trendCount == 0)
                {
                    TempData["IsCrawling"] = true;
                    TempData["TargetTag"] = SearchQuery;

                    if (existingTag == null)
                    {
                        _context.NailTags.Add(new NailTag { TagName = q, ViewCount = 1 });
                        await _context.SaveChangesAsync();
                    }
                    await RunPythonCrawlerAsync(q);
                }
                else
                {
                    existingTag.ViewCount += 1;
                    await _context.SaveChangesAsync();
                }
                query = query.Where(t =>
            t.Tag.ToLower().Trim().Replace(" ", "").Replace("#", "") == q
            );
            }

            else
            {
                CurrentDisplayTag = "All_StylesLabel";
                var topTagNames = AllTags.Take(10)
                                         .Select(t => t.TagName.ToLower().Trim().Replace(" ", "").Replace("#", ""))
                                         .ToList();
                var randomTrends = new List<NailTrend>();
                var random = new Random();

                foreach (var tagName in topTagNames)
                {
                    var tagItems = await _context.NailTrends
                        .Where(t => t.Tag.ToLower().Trim().Replace(" ", "").Replace("#", "") == tagName)
                        .Take(9)
                        .ToListAsync();

                    if (tagItems.Any())
                    {
                        randomTrends.AddRange(tagItems.OrderBy(x => random.Next()).Take(2));
                    }
                }
                var randomIds = randomTrends.Select(t => t.Id).ToList();
                query = query.Where(t => randomIds.Contains(t.Id));
            }

            Debug.WriteLine($"篩選後的 SQL 指令: {query.ToQueryString()}");
            TrendResults = await query.OrderByDescending(t => t.Id).ToListAsync();

            var shopQuery = _context.Shops.Include(s => s.ShopTagBridges).ThenInclude(b => b.NailTag).AsQueryable();
            var regions = _regionService.GetTaiwanRegions();

            if (!string.IsNullOrEmpty(SelectedCity))
            {
                string cleanInputCity = SelectedCity.ToLower().Replace("city", "").Trim();
                var targetCity = regions.FirstOrDefault(r => r.CityEn.ToLower().Contains(cleanInputCity) || r.CityZh.Contains(cleanInputCity));
                if (targetCity != null)
                {
                    var zhCity = targetCity.CityZh;
                    var altZhCity = zhCity.Contains("台") ? zhCity.Replace("台", "臺") : zhCity.Replace("臺", "台");
                    shopQuery = shopQuery.Where(s => s.Address != null && (s.Address.Contains(zhCity) || s.Address.Contains(altZhCity)));
                }
                else shopQuery = shopQuery.Where(s => s.Address != null && s.Address.Contains(SelectedCity));
            }

            if (!string.IsNullOrEmpty(SelectedDistrict))
            {
                string cleanInputDist = SelectedDistrict.ToLower().Replace("district", "").Trim();
                var targetDistrict = regions.SelectMany(r => r.Districts).FirstOrDefault(d => d.En.ToLower().Contains(cleanInputDist) || d.Zh.Contains(cleanInputDist));
                if (targetDistrict != null) shopQuery = shopQuery.Where(s => s.Address != null && s.Address.Contains(targetDistrict.Zh));
            }

            if (SelectedTagId.HasValue)
            {
                var currentTag = AllTags.FirstOrDefault(t => t.Id == SelectedTagId.Value);
                if (currentTag != null)
                {
                    string tagNameLower = currentTag.TagName.ToLower().Replace(" ", "").Replace("#", "");
                    shopQuery = shopQuery.Where(s => s.ShopTagBridges.Any(b => b.NailTag != null && b.NailTag.TagName.ToLower().Replace(" ", "").Replace("#", "") == tagNameLower));
                }
            }

            FeaturedShops = await shopQuery.OrderByDescending(s => s.Id).Take(5).ToListAsync();
            ViewData["SelectedCity"] = SelectedCity;
            ViewData["SelectedDistrict"] = SelectedDistrict;
        }

        private async Task RunPythonCrawlerAsync(string tagToSearch)
        {
            try
            {
                string scriptPath = Path.Combine(Directory.GetCurrentDirectory(), "scripts", "import_sqlite3.py");
                ProcessStartInfo start = new ProcessStartInfo
                {
                    FileName = "python3",
                    Arguments = $"\"{scriptPath}\" \"{tagToSearch}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };
                using (Process process = Process.Start(start)) { await process.WaitForExitAsync(); }
            }
            catch (Exception ex) { Debug.WriteLine($"爬蟲失敗: {ex.Message}"); }
        }
    }
}