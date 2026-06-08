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

        [BindProperty]
        public NewApply ApplyForm { get; set; }

        public List<NailTag> AvailableTags { get; set; }

        [BindProperty]
        public List<int> SelectedTagIds { get; set; }

        public bool IsSuccess { get; set; } = false;

        public async Task OnGetAsync()
        {
            AvailableTags = await _context.NailTags.AsNoTracking().ToListAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                AvailableTags = await _context.NailTags.AsNoTracking().ToListAsync();
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