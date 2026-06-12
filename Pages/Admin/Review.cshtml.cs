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
        private readonly IConfiguration _configuration;

        public ReviewModel(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public IActionResult OnPostVerifyPassword(string adminPassword)
        {
            string correctPassword = _configuration["AdminSettings:SecretPassword"];

            if (string.IsNullOrEmpty(correctPassword))
            {
                Message = "System Error: Admin password is not configured in settings.";
                return RedirectToPage();
            }

            if (adminPassword == correctPassword)
            {
                HttpContext.Session.SetString("AdminLogin", "Success");
                Message = "Welcome back, Admin!";
            }
            else
            {
                Message = "Invalid password. Access denied.";
            }

            return RedirectToPage();
        }

        public IActionResult OnPostLogout()
        {
            HttpContext.Session.Remove("AdminLogin");
            Message = "Logged out successfully.";
            return RedirectToPage();
        }

        public List<NewApply> PendingApplications { get; set; }
        public List<NewApply> AllApplications { get; set; }

        [TempData]
        public string Message { get; set; }

        public async Task OnGetAsync()
        {
            PendingApplications = await _context.NewApplies
                .Where(a => a.Status == "Pending")
                .OrderByDescending(a => a.ApplyDate)
                .AsNoTracking()
                .ToListAsync();

            AllApplications = await _context.NewApplies
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

            string detectedCity = application.City?.Trim();
            string detectedDistrict = application.District?.Trim();
            string currentAddress = application.Address ?? "";

            if (string.IsNullOrEmpty(detectedCity) && currentAddress.Length >= 3)
            {
                detectedCity = currentAddress.Substring(0, 3);
            }

            if (string.IsNullOrEmpty(detectedDistrict) && currentAddress.Length >= 6)
            {
                detectedDistrict = currentAddress.Substring(3, 3);
            }

            if (!string.IsNullOrEmpty(detectedCity) && !detectedCity.Contains("市") && !detectedCity.Contains("縣"))
            {
                detectedCity = "";
            }

            var newShop = new Shop
            {
                ShopName = application.ShopName,
                OwnerName = application.OwnerName,
                Address = application.Address,
                Location = !string.IsNullOrEmpty(detectedCity) ? detectedCity : "未設定",
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
    }
}