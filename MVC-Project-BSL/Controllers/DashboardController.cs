using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MVC_Project_BSL.Data.UnitOfWork;
using MVC_Project_BSL.Models;
using MVC_Project_BSL.ViewModels;
using System.Security.Claims;

namespace MVC_Project_BSL.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public DashboardController(IUnitOfWork unitOfWork)
        {

            _unitOfWork = unitOfWork;
        }
        // GET: Groepsreis
        public async Task<IActionResult> Index()
        {
            // Haal alle groepsreizen op met de bijbehorende monitoren en hun personen
            var groepsreizen = await _unitOfWork.GroepsreisRepository.GetAllAsync(
                query => query.Include(g => g.Monitoren)
                               .ThenInclude(m => m.Persoon) // Persoon van Monitoren ophalen
                               .Include(g => g.Bestemming)
                               .Include(g => g.Kinderen)); // Bestemming van de Groepsreis ophalen

					// Sorteren op Begindatum
					geboekteGroepsreizen = geboekteGroepsreizen.OrderBy(g => g.Begindatum);
					toekomstigeGroepsreizen = toekomstigeGroepsreizen.OrderBy(g => g.Begindatum);

					var alleBestemmingen = await _unitOfWork.BestemmingRepository.GetAllAsync();

					model = new GroepsreisViewModel
					{
						GeboekteGroepsReizen = geboekteGroepsreizen.ToList(),
						ToekomstigeGroepsReizen = toekomstigeGroepsreizen.ToList(),
						AlleBestemmingen = alleBestemmingen.ToList()
					};
				}
			}

            return View(groepsreizen);
        }
    }
}
