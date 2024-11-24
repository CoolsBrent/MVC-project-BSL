using Microsoft.AspNetCore.Identity;
using MVC_Project_BSL.Models;

namespace MVC_Project_BSL.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = serviceProvider.GetRequiredService<UserManager<CustomUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();

            // Zorg ervoor dat de database up-to-date is
            context.Database.EnsureCreated();

            // Rollen toevoegen
            await AddRoles(roleManager);

            // Gebruikers toevoegen
            await AddUsers(userManager);

            // Bestemmingen toevoegen
            AddBestemmingen(context);

            // Groepsreizen toevoegen
            AddGroepsreizen(context);

            // Activiteiten toevoegen
            AddActiviteiten(context);

            // Programma's koppelen aan groepsreizen
            AddProgrammas(context);

            // Foto's toevoegen
            AddFotos(context);

            // Onkosten toevoegen
            AddOnkosten(context);

            // Opslaan in de database
            await context.SaveChangesAsync();
        }

        private static async Task AddRoles(RoleManager<IdentityRole<int>> roleManager)
        {
            string[] roles = { "Deelnemer", "Monitor", "Hoofdmonitor", "Verantwoordelijke", "Beheerder" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole<int> { Name = role });
                }
            }
        }

        private static async Task AddUsers(UserManager<CustomUser> userManager)
        {
            await CreateUser(userManager, "deelnemer@example.com", "Password123!", "Deelnemer", "Geel", "Doe", "Deelnemer");
            await CreateUser(userManager, "monitor@example.com", "Password123!", "Monitor", "Geel", "Smith", "Monitor");
            await CreateUser(userManager, "hoofdmonitor@example.com", "Password123!", "Hoofdmonitor", "Geel", "Brown", "Hoofdmonitor");
            await CreateUser(userManager, "verantwoordelijke@example.com", "Password123!", "Verantwoordelijke", "Geel", "Green", "Verantwoordelijke");
            await CreateUser(userManager, "beheerder@example.com", "Password123!", "Beheerder", "Geel", "Black", "Beheerder");
        }

        private static async Task CreateUser(UserManager<CustomUser> userManager, string email, string password, string role, string gemeente, string naam, string voornaam)
        {
            if (await userManager.FindByEmailAsync(email) == null)
            {
                var user = new CustomUser
                {
                    UserName = email,
                    Email = email,
                    Gemeente = gemeente,
                    Naam = naam,
                    Voornaam = voornaam,
                    Straat = "Straat",
                    Huisnummer = "1",
                    Postcode = "2440",
                    Huisdokter = "Dr. Huisdokter",
                    TelefoonNummer = "014/123456",
                    RekeningNummer = "BE123456789",
                    IsActief = true,
                    Geboortedatum = new DateTime(1990, 1, 1)
                };

                var result = await userManager.CreateAsync(user, password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, role);
                }
            }
        }

        private static void AddBestemmingen(ApplicationDbContext context)
        {
            if (!context.Bestemmingen.Any())
            {
                context.Bestemmingen.AddRange(
                    new Bestemming { Code = "B001", BestemmingsNaam = "Parijs", Beschrijving = "Een reis naar Parijs.", MinLeeftijd = 10, MaxLeeftijd = 18 },
                    new Bestemming { Code = "B002", BestemmingsNaam = "Londen", Beschrijving = "Een culturele trip naar Londen.", MinLeeftijd = 12, MaxLeeftijd = 18 }
                );
            }
        }

        private static void AddGroepsreizen(ApplicationDbContext context)
        {
            if (!context.Groepsreizen.Any())
            {
                context.Groepsreizen.AddRange(
                    new Groepsreis { BestemmingId = 1, Begindatum = new DateTime(2024, 5, 1), Einddatum = new DateTime(2024, 5, 7), Prijs = 500.0f, IsArchived = false },
                    new Groepsreis { BestemmingId = 2, Begindatum = new DateTime(2024, 6, 10), Einddatum = new DateTime(2024, 6, 15), Prijs = 600.0f, IsArchived = false }
                );
            }
        }

        private static void AddActiviteiten(ApplicationDbContext context)
        {
            if (!context.Activiteiten.Any())
            {
                context.Activiteiten.AddRange(
                    new Activiteit { Naam = "Eiffeltoren Bezoek", Beschrijving = "Een bezoek aan de Eiffeltoren." },
                    new Activiteit { Naam = "Musea Bezoeken", Beschrijving = "Ontdek de rijke geschiedenis van Parijs." },
                    new Activiteit { Naam = "Londen Brug", Beschrijving = "Bezoek de iconische brug van Londen." }
                );
            }
        }

        private static void AddProgrammas(ApplicationDbContext context)
        {
            if (!context.Programmas.Any())
            {
                context.Programmas.AddRange(
                    new Programma { ActiviteitId = 1, GroepsreisId = 1 },
                    new Programma { ActiviteitId = 2, GroepsreisId = 1 },
                    new Programma { ActiviteitId = 3, GroepsreisId = 2 }
                );
            }
        }

        private static void AddFotos(ApplicationDbContext context)
        {
            if (!context.Fotos.Any())
            {
                context.Fotos.AddRange(
                    new Foto { Naam = "foto1.jpg", BestemmingId = 1 },
                    new Foto { Naam = "foto2.jpg", BestemmingId = 2 }
                );
            }
        }

        private static void AddOnkosten(ApplicationDbContext context)
        {
            if (!context.Onkosten.Any())
            {
                context.Onkosten.AddRange(
                    new Onkosten { Titel = "Lunch in Parijs", Omschrijving = "Groepslunch tijdens reis.", Bedrag = 150.0f, Datum = new DateTime(2024, 5, 3), Foto = "lunch.jpg", GroepsreisId = 1 },
                    new Onkosten { Titel = "Treintickets", Omschrijving = "Reis naar Londen.", Bedrag = 300.0f, Datum = new DateTime(2024, 6, 11), Foto = null, GroepsreisId = 2 }
                );
            }
        }
    }
}
