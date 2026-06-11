using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NailArtHub.Data;
using NailArtHub.Models;
using NailArtHub.Models.NailArtHub.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NailArtHub.Pages
{
    public class PartnerFormModel : PageModel
    {
        private readonly AppDbContext _context;

        public PartnerFormModel(AppDbContext context)
        {
            _context = context;
        }

        [BindProperty(SupportsGet = true)]
        public string SelectedCity { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SelectedDistrict { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? SelectedTagId { get; set; }

        [BindProperty]
        public NewApply ApplyForm { get; set; }

        public List<NailTag> AvailableTags { get; set; }

        [BindProperty(SupportsGet = true)]
        public List<int> SelectedTagIds { get; set; }

        [BindProperty]
        public string PaymentProofCode { get; set; }

        public bool IsSuccess { get; set; } = false;

        public List<Shop> DisplayShops { get; set; }

        public async Task OnGetAsync()
        {
            AvailableTags = await _context.NailTags
                                 .OrderByDescending(t => t.ViewCount)
                                 .Take(20)
                                 .ToListAsync();

            if (SelectedTagId.HasValue)
            {
                var clickedTag = AvailableTags.FirstOrDefault(t => t.Id == SelectedTagId.Value);
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

            DisplayShops = await shopQuery.ToListAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            int maxAllowedTags = 3;
            if (SelectedTagIds != null && SelectedTagIds.Count > maxAllowedTags)
            {
                ModelState.AddModelError(string.Empty, _localizer["MaxTagsExceededError"]);
            }

            if (!ModelState.IsValid)
            {
                AvailableTags = await _context.NailTags
                                             .AsNoTracking()
                                             .OrderByDescending(t => t.ViewCount)
                                             .Take(20)
                                             .ToListAsync();
                return Page();
            }

            if (SelectedTagIds != null && SelectedTagIds.Any())
            {
                ApplyForm.SelectedTagsString = string.Join(",", SelectedTagIds);
            }

            ApplyForm.City = SelectedCity;
            ApplyForm.District = SelectedDistrict;

            ApplyForm.ApplyDate = System.DateTime.Now;
            ApplyForm.Status = "Pending";

            _context.NewApplies.Add(ApplyForm);
            await _context.SaveChangesAsync();

            IsSuccess = true;
            return Page();
        }
    }
}