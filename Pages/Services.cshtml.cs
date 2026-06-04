using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NailArtHub.Data;
using NailArtHub.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;

namespace NailArtHub.Pages
{
    public class ServicesModel : PageModel
    {
        private readonly AppDbContext _context;

        public ServicesModel(AppDbContext context)
        {
            _context = context;
        }

        public IList<NailTrend> TrendResults { get; set; } = new List<NailTrend>();
        public IList<NailTag> AvailableTags { get; set; } = new List<NailTag>();

        [BindProperty(SupportsGet = true)]
        public string SearchQuery { get; set; }

        [BindProperty(SupportsGet = true)]
        public List<string> SelectedTags { get; set; } = new List<string>();

        public string CurrentDisplayTag { get; set; }

        public async Task OnGetAsync(string handler)
        {
            // Show top 6 only
            AvailableTags = await _context.NailTags
                .OrderByDescending(t => t.ViewCount)
                .Take(6)
                .ToListAsync();

            if (!string.IsNullOrEmpty(SearchQuery) && (SelectedTags == null || !SelectedTags.Any()))
            {
                string cleanedSearch = SearchQuery.Trim().ToLower().Replace(" ", "").Replace("#", "");

                CurrentDisplayTag = SearchQuery.Trim().ToUpper().Replace("#", "");

                var existingTag = await _context.NailTags
                    .FirstOrDefaultAsync(t => t.TagName.ToLower().Replace(" ", "").Replace("#", "") == cleanedSearch);

                if (existingTag == null)
                {
                    var newTag = new NailTag { TagName = SearchQuery.Trim().Replace("#", ""), ViewCount = 1 };
                    _context.NailTags.Add(newTag);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    existingTag.ViewCount += 1;
                }
                await _context.SaveChangesAsync();

                AvailableTags = await _context.NailTags
                    .OrderByDescending(t => t.ViewCount)
                    .Take(6)
                    .ToListAsync();

                int trendCount = await _context.NailTrends.CountAsync(t => t.Tag.ToLower().Trim() == cleanedSearch);
                if (trendCount < 3) // Less 3 then re-scrape again
                {
                    await RunPythonCrawlerAsync(cleanedSearch);
                }
            }

            else if (SelectedTags != null && SelectedTags.Any())
            {
                var firstClickedTag = SelectedTags.First();

                string cleanedTag = firstClickedTag.ToLower().Replace(" ", "").Replace("#", "");

                CurrentDisplayTag = firstClickedTag.Replace("#", "").Trim().ToUpper();

                var clickedTagEntity = await _context.NailTags
                    .FirstOrDefaultAsync(t => t.TagName.ToLower().Replace(" ", "").Replace("#", "") == cleanedTag);
                if (clickedTagEntity != null)
                {
                    clickedTagEntity.ViewCount += 1;
                    await _context.SaveChangesAsync();
                }

                // Less 3 results, re-scrape again
                int existingCount = await _context.NailTrends.CountAsync(t => t.Tag.ToLower().Trim() == cleanedTag);

                if (existingCount < 3)
                {
                    await RunPythonCrawlerAsync(cleanedTag);
                }
            }

            var query = _context.NailTrends.AsQueryable();

            if (SelectedTags != null && SelectedTags.Any())
            {
                var selectedTagsLower = SelectedTags.Select(tag => tag.ToLower().Replace(" ", "").Replace("#", "")).ToList();
                query = query.Where(t => selectedTagsLower.Contains(t.Tag.ToLower().Trim()));
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
        }

        private async Task RunPythonCrawlerAsync(string tagToSearch)
        {
            try
            {
                string shellPath = @"c:\Users\honlo\anaconda3\python.exe";
                string scriptPath = @"C:\Users\honlo\Documents\NailArtHub\import_sqlite3.py";

                ProcessStartInfo start = new ProcessStartInfo();
                start.FileName = shellPath;
                start.Arguments = $"\"{scriptPath}\" \"{tagToSearch}\"";
                start.UseShellExecute = false;
                start.RedirectStandardOutput = true;
                start.RedirectStandardError = true;
                start.CreateNoWindow = true;
                start.StandardOutputEncoding = System.Text.Encoding.UTF8;
                start.StandardErrorEncoding = System.Text.Encoding.UTF8;

                using (Process process = Process.Start(start))
                {
                    string output = await process.StandardOutput.ReadToEndAsync();
                    string error = await process.StandardError.ReadToEndAsync();
                    await process.WaitForExitAsync();
                    System.Diagnostics.Debug.WriteLine("=== Python output ===");
                    System.Diagnostics.Debug.WriteLine(output);
                    if (!string.IsNullOrEmpty(error))
                    {
                        System.Diagnostics.Debug.WriteLine("=== Python error ===");
                        System.Diagnostics.Debug.WriteLine(error);
                    }
                }
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"C# failed: {ex.Message}");
            }
        }
    }
}