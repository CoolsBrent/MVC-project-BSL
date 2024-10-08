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

        public async Task<IActionResult> Index(int? minLeeftijd, int? maxLeeftijd, DateTime? begindatum)
        {
            // Haal alle groepsreizen op inclusief bestemmingen en foto's
            var groepsreizen = await _unitOfWork.GroepsreisRepository.GetAllAsync(
                query => query.Include(g => g.Bestemming)
                               .ThenInclude(b => b.Fotos));

            // Pas filters toe
            if (minLeeftijd.HasValue)
            {
                groepsreizen = groepsreizen.Where(g => g.Bestemming.MinLeeftijd >= minLeeftijd.Value);
            }

            if (maxLeeftijd.HasValue)
            {
                groepsreizen = groepsreizen.Where(g => g.Bestemming.MaxLeeftijd <= maxLeeftijd.Value);
            }

            if (begindatum.HasValue)
            {
                groepsreizen = groepsreizen.Where(g => g.Begindatum >= begindatum.Value);
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
