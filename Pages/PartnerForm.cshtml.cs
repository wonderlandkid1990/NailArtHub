using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
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
        private readonly IStringLocalizer<PartnerFormModel> _localizer;

        public PartnerFormModel(AppDbContext context, IStringLocalizer<PartnerFormModel> localizer)
        {
            _context = context;
            _localizer = localizer;
        }

        [BindProperty(SupportsGet = true)]
        public string? SelectedCity { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? SelectedDistrict { get; set; }

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
                                 .Take(18)
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
            string cityFromForm = Request.Form["SelectedCity"].FirstOrDefault()
                               ?? Request.Form["City"].FirstOrDefault()
                               ?? Request.Form["ApplyForm.City"].FirstOrDefault()
                               ?? SelectedCity;

            string districtFromForm = Request.Form["SelectedDistrict"].FirstOrDefault()
                                   ?? Request.Form["District"].FirstOrDefault()
                                   ?? Request.Form["ApplyForm.District"].FirstOrDefault()
                                   ?? SelectedDistrict;

            string paymentCodeFromForm = Request.Form["PaymentProofCode"].FirstOrDefault()
                                      ?? Request.Form["ApplyForm.PaymentProofCode"].FirstOrDefault()
                                      ?? Request.Form["TradeNo"].FirstOrDefault()
                                      ?? Request.Form["ApplyForm.TradeNo"].FirstOrDefault()
                                      ?? PaymentProofCode;
            if (string.IsNullOrEmpty(cityFromForm))
            {
                ModelState.AddModelError("SelectedCity", "The SelectedCity field is required.");
            }
            if (string.IsNullOrEmpty(districtFromForm))
            {
                ModelState.AddModelError("SelectedDistrict", "The SelectedDistrict field is required.");
            }
            if (ApplyForm != null)
            {
                string baseAddress = ApplyForm.Address ?? "";
                string combinedAddress = $"{cityFromForm}{districtFromForm}{baseAddress}";
                string cleanAddress = System.Text.RegularExpressions.Regex.Replace(combinedAddress, @"^[a-zA-Z\s']+", "").Trim();

                ApplyForm.Address = cleanAddress;
                ApplyForm.City = System.Text.RegularExpressions.Regex.Replace(cityFromForm ?? "", @"^[a-zA-Z\s']+", "").Trim();
                ApplyForm.District = System.Text.RegularExpressions.Regex.Replace(districtFromForm ?? "", @"^[a-zA-Z\s']+", "").Trim();
                ApplyForm.PaymentProofCode = paymentCodeFromForm;
                ApplyForm.Location = "Pending";
            }

            ModelState.Remove("ApplyForm.City");
            ModelState.Remove("ApplyForm.District");
            ModelState.Remove("ApplyForm.Address");
            ModelState.Remove("ApplyForm.Location");
            ModelState.Remove("ApplyForm.PaymentProofCode");
            ModelState.Remove("PaymentProofCode");

            int maxAllowedTags = 3;
            if (SelectedTagIds != null && SelectedTagIds.Count > maxAllowedTags)
            {
                ModelState.AddModelError(string.Empty, _localizer["MaxTagsExceededError", maxAllowedTags]);
            }

            if (!ModelState.IsValid)
            {
                AvailableTags = await _context.NailTags
                                             .AsNoTracking()
                                             .OrderByDescending(t => t.ViewCount)
                                             .Take(18)
                                             .ToListAsync();
                return Page();
            }

            if (SelectedTagIds != null && SelectedTagIds.Any())
            {
                ApplyForm.SelectedTagsString = string.Join(",", SelectedTagIds);
            }

            ApplyForm.ApplyDate = System.DateTime.Now;
            ApplyForm.Status = "Pending";

            try
            {
                _context.NewApplies.Add(ApplyForm);
                await _context.SaveChangesAsync();

                IsSuccess = true;
                return Page();
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                ModelState.AddModelError(string.Empty, "寫入發生異常: " + innerMessage);

                AvailableTags = await _context.NailTags
                                             .AsNoTracking()
                                             .OrderByDescending(t => t.ViewCount)
                                             .Take(18)
                                             .ToListAsync();
                return Page();
            }
        }
    }
}