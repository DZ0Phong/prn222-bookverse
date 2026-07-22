using BookVerse.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.EntityFrameworkCore;
using BookVerse.Configuration;
using BookVerse.Localization;
using BookVerse.Payments;
using BookVerse.Commerce;
using Microsoft.AspNetCore.DataProtection;

namespace BookVerse
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Buyer-editable settings live outside source code. reloadOnChange lets an
            // administrator update the JSON files without rebuilding the application.
            builder.Configuration
                .AddJsonFile("Config/site-settings.json", optional: false, reloadOnChange: true)
                .AddJsonFile("Config/vnpay.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables();

            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();
            builder.Logging.AddDebug();

            // Add services to the container.
            builder.Services.AddControllersWithViews()
                .AddRazorOptions(options =>
                {
                    // Add Admin view locations so MVC finds Views/Admin/{controller}/{action}.cshtml
                    options.ViewLocationFormats.Insert(0, "/Views/Admin/{1}/{0}.cshtml");
                    options.ViewLocationFormats.Insert(1, "/Views/Admin/Shared/{0}.cshtml");
                });

            builder.Services.AddDbContext<QuanLyBanSachContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("MyCnn")));

            builder.Services.Configure<SiteSettings>(builder.Configuration.GetSection("SiteSettings"));
            builder.Services.Configure<VnPayOptions>(builder.Configuration.GetSection("VnPay"));
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddSingleton<IJsonLocalizer, JsonLocalizer>();
            builder.Services.AddScoped<IVnPayService, VnPayService>();
            builder.Services.AddScoped<ICommercePricingService, CommercePricingService>();
            builder.Services.AddDataProtection()
                .SetApplicationName("BookVerse")
                .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, ".data-protection-keys")));

            // Cookie Authentication
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Account/Login";
                    options.AccessDeniedPath = "/Account/AccessDenied";
                    options.ExpireTimeSpan = TimeSpan.FromHours(8);
                    options.SlidingExpiration = true;
                });

            builder.Services.AddAuthorization();

            // Session support
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromHours(8);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseSession();          // Session must come before Authentication
            app.UseAuthentication();
            app.UseAuthorization();

            // Admin area route
            app.MapControllerRoute(
                name: "admin",
                pattern: "Admin/{controller=Dashboard}/{action=Index}/{id?}",
                defaults: new { area = "" });

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=HomePage}/{id?}");

            app.Run();
        }
    }
}
