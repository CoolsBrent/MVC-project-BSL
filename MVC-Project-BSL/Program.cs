using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MVC_Project_BSL.Data;
using MVC_Project_BSL.Data.UnitOfWork;
using MVC_Project_BSL.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

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
builder.Services.AddIdentity<CustomUser, ApplicationRole>(options => options.SignIn.RequireConfirmedAccount = false)
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
await SeedRoles(app.Services);

app.Run();

// Methode om rollen aan te maken
static async Task SeedRoles(IServiceProvider serviceProvider)
{
    // Maak een scope aan om de scoped services te kunnen gebruiken
    using (var scope = serviceProvider.CreateScope())
    {
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<CustomUser>>();

        string[] roleNames = ["Deelnemer", "Monitor", "Hoofdmonitor", "Verantwoordelijke", "Beheerder"];

        foreach (var roleName in roleNames)
        {
            var roleExist = await roleManager.RoleExistsAsync(roleName);
            if (!roleExist)
            {
                await roleManager.CreateAsync(new ApplicationRole(roleName));
            }
        }

        // Optioneel: Als je een standaard gebruiker wilt aanmaken en deze de rol 'Beheerder' wilt geven
        var adminEmail = "admin@mvc";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            var newAdmin = new CustomUser
            {
                UserName = adminEmail,
                Email = adminEmail,
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
                IsActief = true,
            };
            var adminResult = await userManager.CreateAsync(newAdmin, "AdminPassword123!");

            if (adminResult.Succeeded)
            {
                await userManager.AddToRoleAsync(newAdmin, "Beheerder");
            }
        }
    }
}
