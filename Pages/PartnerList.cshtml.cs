using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NailArtHub.Data;
using NailArtHub.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NailArtHub.Pages
{
    public class PartnerListModel : PageModel
    {
        private readonly AppDbContext _context;

        public PartnerListModel(AppDbContext context)
        {
            _context = context;
        }

        public List<Shop> DisplayShops { get; set; }
        // For all the chosen tags
        public List<NailTag> AllTags { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SelectedLocation { get; set; }
        public int? SelectedTagId { get; set; }
        public List<string> LocationList { get; set; }
        public async Task OnGetAsync()
        {
            AllTags = await _context.NailTags.OrderBy(t => t.TagName).ToListAsync();
            LocationList = await _context.Shops
                .Where(s => !string.IsNullOrEmpty(s.Location))
                .Select(s => s.Location)
                .Distinct()
                .OrderBy(l => l)
                .ToListAsync();
            // Shop -> ShopTagBridges -> NailTag
            var query = _context.Shops
                .Include(s => s.ShopTagBridges)
                    .ThenInclude(b => b.NailTag)
                .AsQueryable();

            if (SelectedTagId.HasValue)
            {
                query = query.Where(s => s.ShopTagBridges.Any(b => b.NailTagId == SelectedTagId.Value));
            }
            if (!string.IsNullOrEmpty(SelectedLocation))
            {
                query = query.Where(s => s.Location == SelectedLocation);
            }

            DisplayShops = await query.OrderByDescending(s => s.Id).ToListAsync();
        }
    }
}