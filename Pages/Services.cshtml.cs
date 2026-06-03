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

        // Put the list for tag
        public IList<NailTrend> TrendResults { get; set; } = new List<NailTrend>();
        public IList<NailTag> AvailableTags { get; set; } = new List<NailTag>();

        // Combine the search and tags 
        [BindProperty(SupportsGet = true)]
        public string SearchQuery { get; set; }

        [BindProperty(SupportsGet = true)]
        public List<string> SelectedTags { get; set; } = new List<string>();

        public async Task OnGetAsync()

        {
            AvailableTags = await _context.NailTags.ToListAsync();

            if (SelectedTags != null && SelectedTags.Any())
            {
                var firstClickedTag = SelectedTags.First();
                var cleanedTag = firstClickedTag.ToLower().Replace(" ", "");
                int existingCount = await _context.NailTrends.CountAsync(t => t.Tag.ToLower() == cleanedTag);

                if (existingCount == 0)
                {
                    await RunPythonCrawlerAsync(cleanedTag);
                }
            }
            var query = _context.NailTrends.AsQueryable();

            if (SelectedTags != null && SelectedTags.Any())
            {
                var selectedTagsLower = SelectedTags.Select(tag => tag.ToLower().Replace(" ", "")).ToList();
                query = query.Where(t => selectedTagsLower.Contains(t.Tag.ToLower()));
            }

            if (!string.IsNullOrEmpty(SearchQuery))
            {
                query = query.Where(t => t.Title.Contains(SearchQuery) || t.Tag.Contains(SearchQuery));
            }


            TrendResults = await query.OrderByDescending(t => t.Id).ToListAsync();
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
