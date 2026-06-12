using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NailArtHub.Data;
using NailArtHub.Models;
using NailArtHub.Models.NailArtHub.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NailArtHub.Pages.Admin
{
    public class ReviewModel : PageModel
    {
        private readonly AppDbContext _context;

        public ReviewModel(AppDbContext context)
        {
            _context = context;
        }

        public List<NewApply> PendingApplications { get; set; }

        [TempData]
        public string Message { get; set; }

        public async Task OnGetAsync()
        {
            PendingApplications = await _context.NewApplies
                .Where(a => a.Status == "Pending")
                .OrderByDescending(a => a.ApplyDate)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostApproveAsync(int id)
        {
            var application = await _context.NewApplies.FindAsync(id);
            if (application == null)
            {
                return NotFound();
            }

            application.Status = "Approved";

            string currentAddress = application.Address ?? "";
            string detectedCity = application.City;
            string detectedDistrict = application.District;

            if (string.IsNullOrEmpty(detectedCity) && currentAddress.Length >= 3)
            {
                detectedCity = currentAddress.Substring(0, 3);
            }

            if (string.IsNullOrEmpty(detectedDistrict) && currentAddress.Length >= 6)
            {
                detectedDistrict = currentAddress.Substring(3, 3);
            }

            var newShop = new Shop
            {
                ShopName = application.ShopName,
                OwnerName = application.OwnerName,
                Address = application.Address,
                Location = application.Location,
                InstagramUrl = application.InstagramUrl,
                PinterestUrl = application.PinterestUrl ?? "",
                IsAgreed = true,

                City = detectedCity,
                District = detectedDistrict
            };

            _context.Shops.Add(newShop);
            await _context.SaveChangesAsync();

            if (!string.IsNullOrEmpty(application.SelectedTagsString))
            {
                var tagIds = application.SelectedTagsString
                    .Split(',', StringSplitOptions.RemoveEmptyEntries);

                foreach (var tagIdStr in tagIds)
                {
                    if (int.TryParse(tagIdStr, out int tagId))
                    {
                        var bridge = new ShopTagBridge
                        {
                            ShopId = newShop.Id,
                            NailTagId = tagId
                        };
                        _context.ShopTagBridges.Add(bridge);
                    }
                }

                await _context.SaveChangesAsync();
            }

            Message = $"Approved the application of「{application.ShopName}」! Shop info already updated to ShopList";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRejectAsync(int id)
        {
            var application = await _context.NewApplies.FindAsync(id);
            if (application == null)
            {
                return NotFound();
            }

            application.Status = "Rejected";
            await _context.SaveChangesAsync();

            Message = $"Rejected the application of 「{application.ShopName}」";
            return RedirectToPage();
        }
        public string DbContextEntryValue(int applyId, string columnName)
        {
            try
            {
                var entity = _context.NewApplies.Local.FirstOrDefault(x => x.Id == applyId)
                             ?? _context.NewApplies.FirstOrDefault(x => x.Id == applyId);

                if (entity != null)
                {
                    return _context.Entry(entity).Property(columnName).CurrentValue?.ToString() ?? "";
                }
            }
            catch { }
            return "";
        }
    }
}