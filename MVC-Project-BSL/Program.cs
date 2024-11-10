using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MVC_Project_BSL.Data;
using MVC_Project_BSL.Data.UnitOfWork;
using MVC_Project_BSL.Models;
using MVC_Project_BSL.Services;

public partial class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
		builder.Services.AddScoped<MonitorService>();

		// Add services to the container.
		var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        // Voeg ApplicationDbContext en SQL Server toe
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));

        builder.Services.AddDatabaseDeveloperPageExceptionFilter();

        // Voeg Razor Pages toe
        builder.Services.AddRazorPages();

        // Gebruik CustomUser in plaats van IdentityUser
        builder.Services.AddDefaultIdentity<CustomUser>(options => options.SignIn.RequireConfirmedAccount = false)
            .AddRoles<IdentityRole<int>>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        // Voeg controllers met views toe
        builder.Services.AddControllersWithViews();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseMigrationsEndPoint();
        }
        else
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        // Zorg dat authenticatie en autorisatie wordt gebruikt
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");
        app.MapRazorPages();

        //Aanmaken van rollen bij opstart
        using (var scope = app.Services.CreateScope())
        {
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();

            var roles = new[] { "Deelnemer", "Monitor", "Hoofdmonitor", "Verantwoordelijke", "Beheerder" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole<int>(role));
                }
            }
        }

        //Aanmaken van Admin bij opstart
        using (var scope = app.Services.CreateScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<CustomUser>>();

            string email = "admin@mvc";
            string password = "AdminPassword123!";

            if (await userManager.FindByEmailAsync("admin@mvc") == null)
            {
                var admin = new CustomUser
                {
                    UserName = email,
                    Email = email,
                    Gemeente = "Geel",
                    Naam = "Admin",
                    Voornaam = "Administrator",
                    Straat = "AdminStraat",
                    Huisnummer = "1",
                    Postcode = "2440",
                    Geboortedatum = new DateTime(1990, 1, 1),
                    Huisdokter = "Dr. Admin",
                    TelefoonNummer = "014/123456",
                    RekeningNummer = "BE123456789",
                    IsActief = true
                };

                await userManager.CreateAsync(admin, password);
                await userManager.AddToRoleAsync(admin, "Beheerder");
            }
        }

        app.Run();

    }
}