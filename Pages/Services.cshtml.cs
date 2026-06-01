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

                int existingCount = await _context.NailTrends.CountAsync(t => t.Tag == firstClickedTag);

                if (existingCount == 0)
                {
                    await RunPythonCrawlerAsync(firstClickedTag);
                }
            }
            var query = _context.NailTrends.AsQueryable();

            if (SelectedTags != null && SelectedTags.Any())
            {
                query = query.Where(t => SelectedTags.Contains(t.Tag));
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
                string pythonExePath = @"python";
                string scriptPath = @"C:\Users\honlo\Documents\NailArtHub\import_sqlite3.py";

                ProcessStartInfo start = new ProcessStartInfo();
                start.FileName = pythonExePath;
                start.Arguments = $"\"{scriptPath}\" \"{tagToSearch}\"";
                start.UseShellExecute = false;
                start.RedirectStandardOutput = true;
                start.CreateNoWindow = true;

                using (Process process = Process.Start(start))
                {
                    await process.WaitForExitAsync();
                }
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"Python 執行失敗: {ex.Message}");
            }
        }
    }
}
