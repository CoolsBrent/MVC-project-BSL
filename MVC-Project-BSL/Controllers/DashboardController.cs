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
        private readonly UserManager<CustomUser> _userManager;

        public DashboardController(IUnitOfWork unitOfWork, UserManager<CustomUser> userManager)
        {

            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }
		// GET: Groepsreis
		public async Task<IActionResult> Index(string bestemming)
		{
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

			if (string.IsNullOrEmpty(userId))
			{
				return Unauthorized(); // Als het gebruikers-ID niet beschikbaar is
			}

			var gebruiker = await _userManager.FindByIdAsync(userId);
			if (gebruiker == null)
			{
				return NotFound("Gebruiker niet gevonden."); // Als de gebruiker niet gevonden wordt
			}

			// Controleer of de gebruiker een beheerder is
			bool isBeheerder = await _userManager.IsInRoleAsync(gebruiker, "Beheerder");
			bool isMonitor = await _userManager.IsInRoleAsync(gebruiker, "Monitor");

			GroepsreisViewModel model;

			if (isBeheerder)
			{
				// Haal alle groepsreizen op voor beheerders, inclusief filters
				var alleGroepsreizen = await _unitOfWork.GroepsreisRepository.GetAllAsync(query =>
					query.Include(g => g.Deelnemers)
							.ThenInclude(d => d.Kind)
							.Include(g => g.Monitoren)
							.Include(g => g.Bestemming)
							.ThenInclude(b => b.Fotos)
				);

				// Filter op bestemming als deze is opgegeven
				if (!string.IsNullOrEmpty(bestemming))
				{
					alleGroepsreizen = alleGroepsreizen.Where(g => g.Bestemming.BestemmingsNaam == bestemming);
				}

				alleGroepsreizen = alleGroepsreizen.OrderBy(g => g.Begindatum);
				var alleBestemmingen = await _unitOfWork.BestemmingRepository.GetAllAsync();

				model = new GroepsreisViewModel
				{
					AlleGroepsReizen = alleGroepsreizen.ToList(),
					AlleBestemmingen = alleBestemmingen.ToList()
				};
			}
			else if (isMonitor)
			{
				// Haal alle ingeschreven groepsreizen op waarvoor de gebruiker als monitor is aangesteld
				var geboekteGroepsreizen = await _unitOfWork.GroepsreisRepository.GetAllAsync(query =>
					query.Include(g => g.Deelnemers)
							.ThenInclude(d => d.Kind)
							.Include(g => g.Monitoren)
							.Include(g => g.Bestemming)
							.ThenInclude(b => b.Fotos)
							.Where(g => g.Monitoren.Any(m => m.Monitor.PersoonId == gebruiker.Id)) // Alleen groepsreizen waarvoor de monitor is aangesteld
				);

				// Haal toekomstige groepsreizen op waarvoor de gebruiker als monitor is aangesteld
				var toekomstigeGroepsreizen = await _unitOfWork.GroepsreisRepository.GetAllAsync(query =>
					query.Include(g => g.Deelnemers)
							.ThenInclude(d => d.Kind)
							.Include(g => g.Monitoren)
							.Include(g => g.Bestemming)
							.ThenInclude(b => b.Fotos)
							.Where(g => g.Begindatum > DateTime.Now && g.Monitoren.Any(m => m.Monitor.PersoonId == gebruiker.Id)) // Alleen toekomstige reizen waarvoor de monitor is aangesteld
				);

				// Filter op bestemming als deze is opgegeven
				if (!string.IsNullOrEmpty(bestemming))
				{
					geboekteGroepsreizen = geboekteGroepsreizen.Where(g => g.Bestemming.BestemmingsNaam == bestemming).ToList();
					toekomstigeGroepsreizen = toekomstigeGroepsreizen.Where(g => g.Bestemming.BestemmingsNaam == bestemming).ToList();
				}

				// Sorteren op Begindatum
				geboekteGroepsreizen = geboekteGroepsreizen.OrderBy(g => g.Begindatum).ToList();
				toekomstigeGroepsreizen = toekomstigeGroepsreizen.OrderBy(g => g.Begindatum).ToList();

				var alleBestemmingen = await _unitOfWork.BestemmingRepository.GetAllAsync();

				model = new GroepsreisViewModel
				{
					GeboekteGroepsReizen = geboekteGroepsreizen.ToList(),
					ToekomstigeGroepsReizen = toekomstigeGroepsreizen.ToList(),
					AlleBestemmingen = alleBestemmingen.ToList()
				};
			}


			else
			{
				// Haal alle kinderen op die aan de ingelogde gebruiker zijn gekoppeld
				var kinderen = await _unitOfWork.KindRepository.GetAllAsync();
				var gebruikersKinderen = kinderen.Where(k => k.PersoonId == gebruiker.Id).ToList();

				if (!gebruikersKinderen.Any())
				{
					// Geen kinderen gevonden, dus we halen de toekomstige groepsreizen op
					var toekomstigeGroepsreizen = await _unitOfWork.GroepsreisRepository.GetAllAsync(query =>
						query.Include(g => g.Deelnemers)
								.ThenInclude(d => d.Kind)
								.Include(g => g.Monitoren)
								.Include(g => g.Bestemming)
								.ThenInclude(b => b.Fotos)
								.Where(g => g.Begindatum > DateTime.Now) // Haal alleen toekomstige reizen op
					);

					// Filter op bestemming als deze is opgegeven
					if (!string.IsNullOrEmpty(bestemming))
					{
						toekomstigeGroepsreizen = toekomstigeGroepsreizen.Where(g => g.Bestemming.BestemmingsNaam == bestemming);
					}

					// Sorteren op Begindatum
					toekomstigeGroepsreizen = toekomstigeGroepsreizen.OrderBy(g => g.Begindatum);

					var alleBestemmingen = await _unitOfWork.BestemmingRepository.GetAllAsync();

					model = new GroepsreisViewModel
					{
						ToekomstigeGroepsReizen = toekomstigeGroepsreizen.ToList(),
						AlleBestemmingen = alleBestemmingen.ToList()
					};
				}
				else
				{
					// Haal de groepsreizen op waarin de kinderen zijn ingeschreven, inclusief filters
					var geboekteGroepsreizen = await _unitOfWork.GroepsreisRepository.GetAllAsync(query =>
						query.Include(g => g.Deelnemers)
								.ThenInclude(d => d.Kind)
								.Include(g => g.Monitoren)
								.Include(g => g.Bestemming)
								.ThenInclude(b => b.Fotos)
								.Where(g => g.Deelnemers.Any(d => gebruikersKinderen.Select(k => k.Id).Contains(d.KindId)))
					);

					// Haal de toekomstige groepsreizen op, inclusief filters
					var toekomstigeGroepsreizen = await _unitOfWork.GroepsreisRepository.GetAllAsync(query =>
						query.Include(g => g.Deelnemers)
								.ThenInclude(d => d.Kind)
								.Include(g => g.Monitoren)
								.Include(g => g.Bestemming)
								.ThenInclude(b => b.Fotos)
								.Where(g => g.Begindatum > DateTime.Now)
					);

					// Filter op bestemming als deze is opgegeven
					if (!string.IsNullOrEmpty(bestemming))
					{
						geboekteGroepsreizen = geboekteGroepsreizen.Where(g => g.Bestemming.BestemmingsNaam == bestemming);
						toekomstigeGroepsreizen = toekomstigeGroepsreizen.Where(g => g.Bestemming.BestemmingsNaam == bestemming);
						
					}

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

			return View(model);
		}


	}
}
