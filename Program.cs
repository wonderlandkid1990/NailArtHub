using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using NailArtHub.Data;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSession(options => {
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
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

    var dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "NailArtHub.db");
Console.WriteLine("The DB path now：" + dbPath );
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer("Data Source=SQL6034.site4now.net;Initial Catalog=db_aca7c2_nail;User Id=db_aca7c2_nail_admin;Password=Aliceyu19901103;Encrypt=True;TrustServerCertificate=True;"));

builder.Services.AddScoped<NailArtHub.Services.RegionService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseSession();
app.UseRequestLocalization();

app.UseAuthorization();

app.MapRazorPages();
app.UseStaticFiles();
app.Run();