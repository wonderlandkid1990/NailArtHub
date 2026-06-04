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

        // For panding data
        public List<NewApply> PendingApplications { get; set; }

        // Massage agter suscessed
        [TempData]
        public string Message { get; set; }

        public async Task OnGetAsync()
        {
            // Only pull "Pending" application, from new to old
            PendingApplications = await _context.NewApplies
                .Where(a => a.Status == "Pending")
                .OrderByDescending(a => a.ApplyDate)
                .AsNoTracking()
                .ToListAsync();
        }

        // Approve
        public async Task<IActionResult> OnPostApproveAsync(int id)
        {
            var application = await _context.NewApplies.FindAsync(id);
            if (application == null)
            {
                return NotFound();
            }

            // Upload status
            application.Status = "Approved";

            // Copy application data and build new shop info
            var newShop = new Shop
            {
                ShopName = application.ShopName,
                OwnerName = application.OwnerName,
                Address = application.Address,
                Location = application.Location,
                InstagramUrl = application.InstagramUrl,
                PinterestUrl = application.PinterestUrl ?? "",
                IsAgreed = true
            };

            // Add new shop to Shops table
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

                // Put all bridge to DB
                await _context.SaveChangesAsync();
            }

            Message = $"Approved the application of「{application.ShopName}」! Shop info already updated to ShopList";
            return RedirectToPage();
        }

        // Reject
        public async Task<IActionResult> OnPostRejectAsync(int id)
        {
            var application = await _context.NewApplies.FindAsync(id);
            if (application == null)
            {
                return NotFound();
            }

            // Only update status to Rejected, no move data
            application.Status = "Rejected";
            await _context.SaveChangesAsync();

            Message = $"Rejected the application of 「{application.ShopName}」";
            return RedirectToPage();
        }
    }
}