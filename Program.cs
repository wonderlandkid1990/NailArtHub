using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using NailArtHub.Data;

var builder = WebApplication.CreateBuilder(args);

// ===  System Language (zh / en) === 
var supportedCultures = new[] { "zh", "en" };
builder.Services.Configure<RequestLocalizationOptions>(options => {
    var supportedCultures = new[] { "zh", "en" };
    options.SetDefaultCulture("zh")
           .AddSupportedCultures(supportedCultures)
           .AddSupportedUICultures(supportedCultures);

    options.RequestCultureProviders.Clear();
    options.RequestCultureProviders.Add(new QueryStringRequestCultureProvider { QueryStringKey = "culture" });
    options.RequestCultureProviders.Add(new CookieRequestCultureProvider());
});

builder.Services.AddLocalization();

builder.Services.AddRazorPages()
                .AddViewLocalization(Microsoft.AspNetCore.Mvc.Razor.LanguageViewLocationExpanderFormat.Suffix);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<NailArtHub.Services.RegionService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseRequestLocalization();

app.UseAuthorization();
app.MapStaticAssets();

app.MapRazorPages()
   .WithStaticAssets();

app.Run();