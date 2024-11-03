using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MVC_Project_BSL.Data.UnitOfWork;
using MVC_Project_BSL.Models;
using MVC_Project_BSL.ViewModels;
using System.Diagnostics;

namespace MVC_Project_BSL.Controllers
{
    public class HomeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<HomeController> _logger;

        public HomeController(IUnitOfWork unitOfWork, ILogger<HomeController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        // GET: Home/Index met filters voor leeftijdscategorie, begindatum, en prijsbereik
        public async Task<IActionResult> Index(string leeftijdscategorie, DateTime? begindatum, decimal? maxPrijs)
        {
            // Haal alle groepsreizen op inclusief bestemmingen en foto's
            var groepsreizen = await _unitOfWork.GroepsreisRepository.GetAllAsync(
                query => query.Include(g => g.Bestemming)
                              .ThenInclude(b => b.Fotos));

            // Haal unieke leeftijdscategorieën op uit de database
            var leeftijdscategorieën = groepsreizen
                .Select(g => new { Min = g.Bestemming.MinLeeftijd, Max = g.Bestemming.MaxLeeftijd })
                .Distinct()
                .ToList();

            // Pas filters toe op basis van leeftijdscategorie
            if (!string.IsNullOrEmpty(leeftijdscategorie))
            {
                // Splits de leeftijdscategorie op basis van het streepje, bijvoorbeeld "6-12"
                var leeftijdsBereik = leeftijdscategorie.Split('-');
                int minLeeftijd = int.Parse(leeftijdsBereik[0]);
                int maxLeeftijd = int.Parse(leeftijdsBereik[1]);

                // Filter de groepsreizen binnen het opgegeven leeftijdsbereik
                groepsreizen = groepsreizen.Where(g => g.Bestemming.MinLeeftijd <= maxLeeftijd && g.Bestemming.MaxLeeftijd >= minLeeftijd);
            }

            // Pas filter toe op basis van begindatum
            if (begindatum.HasValue)
            {
                groepsreizen = groepsreizen.Where(g => g.Begindatum >= begindatum.Value);
            }

            // Pas filters toe op basis van prijsbereik
            if (maxPrijs.HasValue)
            {
                groepsreizen = groepsreizen.Where(g => (decimal)g.Prijs <= maxPrijs.Value);
            }

            // Map de groepsreizen naar het ViewModel
            var viewModel = groepsreizen.Select(g => new GroepsreisViewModel
            {
                Id = g.Id,
                Begindatum = g.Begindatum,
                Einddatum = g.Einddatum,
                Bestemming = g.Bestemming.BestemmingsNaam,
                Beschrijving = g.Bestemming.Beschrijving,
                MinLeeftijd = g.Bestemming.MinLeeftijd,
                MaxLeeftijd = g.Bestemming.MaxLeeftijd,
                FotoUrls = g.Bestemming.Fotos.Select(f => f.Naam).ToList(),
                Prijs = (decimal)g.Prijs,
            }).ToList();

            // Maak een ViewBag of een ander ViewModel om de leeftijdscategorieën naar de view te sturen
            ViewBag.Leeftijdscategorieën = leeftijdscategorieën;

            return View(viewModel);
        }



        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
