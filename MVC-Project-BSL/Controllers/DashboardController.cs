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
	/// <summary>
	/// Een controller voor het beheren en weergeven van het dashboard, aangepast aan de rol van de ingelogde gebruiker.
	/// Voor beheerders, monitoren en deelnemers worden relevante groepsreizen en bestemmingen weergegeven, 
	/// waarbij verschillende filters en archiveringsstatussen worden toegepast.
	/// </summary>
	[Authorize]
	public class DashboardController : Controller
	{
		#region Private Fields
		private readonly IUnitOfWork _unitOfWork;
		private readonly UserManager<CustomUser> _userManager;
		#endregion

		#region Constructor
		public DashboardController(IUnitOfWork unitOfWork, UserManager<CustomUser> userManager)
		{
			_unitOfWork = unitOfWork;
			_userManager = userManager;
		}
		#endregion

		#region Index
		// GET: Groepsreis
		public async Task<IActionResult> Index(string bestemming, string filterType)
		{
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _userManager.GetUserAsync(User); // Haal de CustomUser op
            var userName = user != null ? user.Voornaam + " " + user.Naam : "Onbekende gebruiker"; // Stel een fallback in als er geen gebruiker is

            ViewData["UserName"] = userName; // Zet de naam in ViewData
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
			bool isVerantwoordelijke = await _userManager.IsInRoleAsync(gebruiker, "Verantwoordelijke");
			bool isMonitor = await _userManager.IsInRoleAsync(gebruiker, "Monitor");
			bool isHoofdmonitor = await _userManager.IsInRoleAsync(gebruiker, "Hoofdmonitor");

			GroepsreisViewModel model;

			if (isBeheerder || isVerantwoordelijke)
			{
				// Haal alle groepsreizen op voor beheerders, inclusief filters
				var alleGroepsreizen = await _unitOfWork.GroepsreisRepository.GetAllAsync(query =>
					query.Include(g => g.Deelnemers!)
							.ThenInclude(d => d.Kind)
							.Include(g => g.Monitoren)
							.Include(g => g.Bestemming)
							.ThenInclude(b => b!.Fotos)
							.Where(g => !g.IsArchived) // Haal alleen niet-gearchiveerde groepsreizen op
				);
				var gearchiveerdeGroepsreizen = await _unitOfWork.GroepsreisRepository.GetAllAsync(query =>
					query.Include(g => g.Deelnemers!)
							.ThenInclude(d => d.Kind)
							.Include(g => g.Monitoren)
							.Include(g => g.Bestemming)
							.ThenInclude(b => b!.Fotos)
							.Where(g => g.IsArchived) // Haal alleen gearchiveerde groepsreizen op
				);
				var alleOpleidingen = await _unitOfWork.OpleidingRepository.GetAllAsync(query =>
				query.Include(o => o.OpleidingPersonen));
				// Filter op basis van 'filterType'
				if (filterType == "active")
				{
					alleGroepsreizen = alleGroepsreizen.Where(g => !g.IsArchived);
				}
				else if (filterType == "archived")
				{
					gearchiveerdeGroepsreizen = gearchiveerdeGroepsreizen.Where(g => g.IsArchived);
				}
				//Filter op bestemming als deze is opgegeven
				if (!string.IsNullOrEmpty(bestemming))
				{
					alleGroepsreizen = alleGroepsreizen.Where(g => g.Bestemming?.BestemmingsNaam == bestemming);
					gearchiveerdeGroepsreizen = gearchiveerdeGroepsreizen.Where(g => g.Bestemming?.BestemmingsNaam == bestemming);
				}

				alleGroepsreizen = alleGroepsreizen.OrderBy(g => g.Begindatum);
				var alleBestemmingen = await _unitOfWork.BestemmingRepository.GetAllAsync();

				model = new GroepsreisViewModel
				{
					AlleGroepsReizen = alleGroepsreizen.ToList(),
					AlleBestemmingen = alleBestemmingen.ToList(),
					GearchiveerdeGroepsreizen = gearchiveerdeGroepsreizen.ToList(),
					AlleOpleidingen = alleOpleidingen.ToList()
				};
			}
			else if (isMonitor || isHoofdmonitor)
			{
				// Haal alle ingeschreven groepsreizen op waarvoor de gebruiker als monitor is aangesteld
				var geboekteGroepsreizen = await _unitOfWork.GroepsreisRepository.GetAllAsync(query =>
					query.Include(g => g.Deelnemers!)
							.ThenInclude(d => d.Kind)
							.Include(g => g.Monitoren)
							.Include(g => g.Bestemming)
							.ThenInclude(b => b!.Fotos)
							.Where(g => g.Monitoren!.Any(m => m.Monitor.PersoonId == gebruiker.Id) && !g.IsArchived) // Alleen groepsreizen waarvoor de monitor is aangesteld
				);

				// Haal toekomstige groepsreizen op waarvoor de gebruiker als monitor is aangesteld
				var toekomstigeGroepsreizen = await _unitOfWork.GroepsreisRepository.GetAllAsync(query =>
					query.Include(g => g.Deelnemers!)
							.ThenInclude(d => d.Kind)
							.Include(g => g.Monitoren)
							.Include(g => g.Bestemming)
							.ThenInclude(b => b!.Fotos)
							.Where(g => g.Begindatum > DateTime.Now && !g.IsArchived) // Alleen toekomstige reizen waarvoor de monitor is aangesteld
				);
				var toekomstigeOpleidingen = await _unitOfWork.OpleidingRepository.GetAllAsync(query =>
					query.Include(o => o.OpleidingPersonen)
							.Where(o => o.Begindatum > DateTime.Now && !o.OpleidingPersonen.Any(op => op.PersoonId == gebruiker.Id)));
				var ingeschrevenOpleidingen = await _unitOfWork.OpleidingRepository.GetAllAsync(query =>
					query.Include(o => o.OpleidingPersonen)
							.Where(o => o.OpleidingPersonen.Any(op => op.PersoonId == gebruiker.Id) && o.Einddatum > DateTime.Now));
				if (filterType == "mijnReizen")
				{
					geboekteGroepsreizen = geboekteGroepsreizen.Where(g => g.Monitoren!.Any(m => m.Monitor.PersoonId == gebruiker.Id) && !g.IsArchived);
				}
				else if (filterType == "teOntdekkenReizen")
				{
					toekomstigeGroepsreizen = toekomstigeGroepsreizen.Where(g => g.Begindatum > DateTime.Now);
				}
				// Filter op bestemming als deze is opgegeven
				if (!string.IsNullOrEmpty(bestemming))
				{
					geboekteGroepsreizen = geboekteGroepsreizen.Where(g => g.Bestemming!.BestemmingsNaam == bestemming).ToList();
					toekomstigeGroepsreizen = toekomstigeGroepsreizen.Where(g => g.Bestemming!.BestemmingsNaam == bestemming).ToList();
				}

				// Sorteren op Begindatum
				geboekteGroepsreizen = geboekteGroepsreizen.OrderBy(g => g.Begindatum).ToList();
				toekomstigeGroepsreizen = toekomstigeGroepsreizen.OrderBy(g => g.Begindatum).ToList();

				var alleBestemmingen = await _unitOfWork.BestemmingRepository.GetAllAsync();

				model = new GroepsreisViewModel
				{
					GeboekteGroepsReizen = geboekteGroepsreizen.ToList(),
					ToekomstigeGroepsReizen = toekomstigeGroepsreizen.ToList(),
					AlleBestemmingen = alleBestemmingen.ToList(),
					IngeschrevenOpleidingen = ingeschrevenOpleidingen.ToList(),
					ToekomstigeOpleidingen = toekomstigeOpleidingen.ToList()
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
						query.Include(g => g.Deelnemers!)
								.ThenInclude(d => d.Kind)
								.Include(g => g.Monitoren)
								.Include(g => g.Bestemming)
								.ThenInclude(b => b!.Fotos)
								.Where(g => g.Begindatum > DateTime.Now && !g.IsArchived) // Haal alleen toekomstige reizen op
					);

					if (filterType == "teOntdekkenReizen")
					{
						toekomstigeGroepsreizen = toekomstigeGroepsreizen.Where(g => g.Begindatum > DateTime.Now);
					}
					// Filter op bestemming als deze is opgegeven
					if (!string.IsNullOrEmpty(bestemming))
					{
						toekomstigeGroepsreizen = toekomstigeGroepsreizen.Where(g => g.Bestemming!.BestemmingsNaam == bestemming);
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
						query.Include(g => g.Deelnemers!)
								.ThenInclude(d => d.Kind)
								.Include(g => g.Monitoren)
								.Include(g => g.Bestemming)
								.ThenInclude(b => b!.Fotos)
								.Where(g => g.Deelnemers!.Any(d => gebruikersKinderen.Select(k => k.Id).Contains(d.KindId) && !g.IsArchived))
					);

					// Haal de toekomstige groepsreizen op, inclusief filters
					var toekomstigeGroepsreizen = await _unitOfWork.GroepsreisRepository.GetAllAsync(query =>
						query.Include(g => g.Deelnemers!)
								.ThenInclude(d => d.Kind)
								.Include(g => g.Monitoren)
								.Include(g => g.Bestemming)
								.ThenInclude(b => b!.Fotos)
								.Where(g => g.Begindatum > DateTime.Now && !g.IsArchived)
					);
					if (filterType == "mijnReizen")
					{
						geboekteGroepsreizen = geboekteGroepsreizen.Where(g => g.Monitoren!.Any(m => m.Monitor.PersoonId == gebruiker.Id) && !g.IsArchived);
					}
					else if (filterType == "teOntdekkenReizen")
					{
						toekomstigeGroepsreizen = toekomstigeGroepsreizen.Where(g => g.Begindatum > DateTime.Now);
					}
					// Filter op bestemming als deze is opgegeven
					if (!string.IsNullOrEmpty(bestemming))
					{
						geboekteGroepsreizen = geboekteGroepsreizen.Where(g => g.Bestemming!.BestemmingsNaam == bestemming);
						toekomstigeGroepsreizen = toekomstigeGroepsreizen.Where(g => g.Bestemming!.BestemmingsNaam == bestemming);

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
		#endregion

		#region Public Methods

		[HttpPost]
		public async Task<IActionResult> VoegReviewToe(int deelnemerId, int score, string? opmerkingen)
		{
			// Gebruik de async methode voor het ophalen van de deelnemer
			var deelnemer = await _unitOfWork.DeelnemerRepository.GetByIdAsync(deelnemerId);
			if (deelnemer == null)
			{
				return NotFound("Deelnemer niet gevonden.");
			}

			// Update reviewscore en opmerkingen
			deelnemer.ReviewScore = score;
			deelnemer.Review = opmerkingen;
			try
			{
				_unitOfWork.DeelnemerRepository.Update(deelnemer);
				_unitOfWork.SaveChanges(); // Synchronous save, blijft werken
			}
			catch (Exception)
			{
				// Log exception indien nodig
				return StatusCode(500, "Er is een fout opgetreden bij het opslaan van de review.");
			}

			return RedirectToAction("Index");
		}

		[HttpPost]
		public async Task<IActionResult> VerwijderReview(int deelnemerId)
		{
			// Haal de deelnemer op via het ID
			var deelnemer = await _unitOfWork.DeelnemerRepository.GetByIdAsync(deelnemerId);
			if (deelnemer == null)
			{
				return NotFound("Deelnemer niet gevonden.");
			}

			// Reset de review en score
			deelnemer.ReviewScore = null;
			deelnemer.Review = null;

			try
			{
				_unitOfWork.DeelnemerRepository.Update(deelnemer);
				_unitOfWork.SaveChanges(); // Zorg dat wijzigingen worden opgeslagen
			}
			catch (Exception ex)
			{
				// Voeg logging toe indien nodig
				return StatusCode(500, "Er is een fout opgetreden bij het verwijderen van de review.");
			}

			// Keer terug naar dezelfde pagina
			return RedirectToAction("Index");
		}

		#endregion

		#region Private Methods

		private async Task<GroepsreisViewModel> LoadGroepsreizenForBeheerder(string bestemming)
		{
			var alleGroepsreizen = await _unitOfWork.GroepsreisRepository.GetAllAsync(query =>
				query.Include(g => g.Deelnemers!)
					 .ThenInclude(d => d.Kind)
					 .Include(g => g.Monitoren)
					 .Include(g => g.Bestemming)
					 .ThenInclude(b => b!.Fotos)
					 .Where(g => !g.IsArchived));

			var gearchriveerdeGroepsreizen = await _unitOfWork.GroepsreisRepository.GetAllAsync(query =>
				query.Include(g => g.Deelnemers!)
					 .ThenInclude(d => d.Kind)
					 .Include(g => g.Monitoren)
					 .Include(g => g.Bestemming)
					 .ThenInclude(b => b!.Fotos)
					 .Where(g => g.IsArchived));

			if (!string.IsNullOrEmpty(bestemming))
			{
				alleGroepsreizen = alleGroepsreizen.Where(g => g.Bestemming!.BestemmingsNaam == bestemming);
				gearchriveerdeGroepsreizen = gearchriveerdeGroepsreizen.Where(g => g.Bestemming!.BestemmingsNaam == bestemming);
			}

			alleGroepsreizen = alleGroepsreizen.OrderBy(g => g.Begindatum);
			var alleBestemmingen = await _unitOfWork.BestemmingRepository.GetAllAsync();

			return new GroepsreisViewModel
			{
				AlleGroepsReizen = alleGroepsreizen.ToList(),
				AlleBestemmingen = alleBestemmingen.ToList(),
				GearchiveerdeGroepsreizen = gearchriveerdeGroepsreizen.ToList()
			};
		}

		private async Task<GroepsreisViewModel> LoadGroepsreizenForMonitor(int userId, string bestemming)
		{
			var geboekteGroepsreizen = await _unitOfWork.GroepsreisRepository.GetAllAsync(query =>
				query.Include(g => g.Deelnemers!)
					 .ThenInclude(d => d.Kind)
					 .Include(g => g.Monitoren)
					 .Include(g => g.Bestemming)
					 .ThenInclude(b => b!.Fotos)
					 .Where(g => g.Monitoren!.Any(m => m.Monitor.PersoonId == userId) && !g.IsArchived));

			var toekomstigeGroepsreizen = await _unitOfWork.GroepsreisRepository.GetAllAsync(query =>
				query.Include(g => g.Deelnemers!)
					 .ThenInclude(d => d.Kind)
					 .Include(g => g.Monitoren)
					 .Include(g => g.Bestemming)
					 .ThenInclude(b => b!.Fotos)
					 .Where(g => g.Begindatum > DateTime.Now && !g.IsArchived));

			if (!string.IsNullOrEmpty(bestemming))
			{
				geboekteGroepsreizen = geboekteGroepsreizen.Where(g => g.Bestemming!.BestemmingsNaam == bestemming).ToList();
				toekomstigeGroepsreizen = toekomstigeGroepsreizen.Where(g => g.Bestemming!.BestemmingsNaam == bestemming).ToList();
			}

			geboekteGroepsreizen = geboekteGroepsreizen.OrderBy(g => g.Begindatum).ToList();
			toekomstigeGroepsreizen = toekomstigeGroepsreizen.OrderBy(g => g.Begindatum).ToList();

			var alleBestemmingen = await _unitOfWork.BestemmingRepository.GetAllAsync();

			return new GroepsreisViewModel
			{
				GeboekteGroepsReizen = geboekteGroepsreizen.ToList(),
				ToekomstigeGroepsReizen = toekomstigeGroepsreizen.ToList(),
				AlleBestemmingen = alleBestemmingen.ToList()
			};
		}

		private async Task<GroepsreisViewModel> LoadGroepsreizenForUser(int userId, string bestemming)
		{
			var kinderen = await _unitOfWork.KindRepository.GetAllAsync();
			var gebruikersKinderen = kinderen.Where(k => k.PersoonId == userId).ToList();

			if (!gebruikersKinderen.Any())
			{
				var toekomstigeGroepsreizen = await _unitOfWork.GroepsreisRepository.GetAllAsync(query =>
					query.Include(g => g.Deelnemers!)
						 .ThenInclude(d => d.Kind)
						 .Include(g => g.Monitoren)
						 .Include(g => g.Bestemming)
						 .ThenInclude(b => b!.Fotos)
						 .Where(g => g.Begindatum > DateTime.Now && !g.IsArchived));

				if (!string.IsNullOrEmpty(bestemming))
				{
					toekomstigeGroepsreizen = toekomstigeGroepsreizen.Where(g => g.Bestemming!.BestemmingsNaam == bestemming);
				}

				toekomstigeGroepsreizen = toekomstigeGroepsreizen.OrderBy(g => g.Begindatum);

				var alleBestemmingen = await _unitOfWork.BestemmingRepository.GetAllAsync();

				return new GroepsreisViewModel
				{
					ToekomstigeGroepsReizen = toekomstigeGroepsreizen.ToList(),
					AlleBestemmingen = alleBestemmingen.ToList()
				};
			}
			else
			{
				var geboekteGroepsreizen = await _unitOfWork.GroepsreisRepository.GetAllAsync(query =>
					query.Include(g => g.Deelnemers!)
						 .ThenInclude(d => d.Kind)
						 .Include(g => g.Monitoren)
						 .Include(g => g.Bestemming)
						 .ThenInclude(b => b!.Fotos)
						 .Where(g => g.Deelnemers!.Any(d => gebruikersKinderen.Select(k => k.Id).Contains(d.KindId) && !g.IsArchived)));

				var toekomstigeGroepsreizen = await _unitOfWork.GroepsreisRepository.GetAllAsync(query =>
					query.Include(g => g.Deelnemers!)
						 .ThenInclude(d => d.Kind)
						 .Include(g => g.Monitoren)
						 .Include(g => g.Bestemming)
						 .ThenInclude(b => b!.Fotos)
						 .Where(g => g.Begindatum > DateTime.Now && !g.IsArchived));

				if (!string.IsNullOrEmpty(bestemming))
				{
					geboekteGroepsreizen = geboekteGroepsreizen.Where(g => g.Bestemming!.BestemmingsNaam == bestemming);
					toekomstigeGroepsreizen = toekomstigeGroepsreizen.Where(g => g.Bestemming!.BestemmingsNaam == bestemming);
				}

				geboekteGroepsreizen = geboekteGroepsreizen.OrderBy(g => g.Begindatum);
				toekomstigeGroepsreizen = toekomstigeGroepsreizen.OrderBy(g => g.Begindatum);

				var alleBestemmingen = await _unitOfWork.BestemmingRepository.GetAllAsync();

				return new GroepsreisViewModel
				{
					GeboekteGroepsReizen = geboekteGroepsreizen.ToList(),
					ToekomstigeGroepsReizen = toekomstigeGroepsreizen.ToList(),
					AlleBestemmingen = alleBestemmingen.ToList()
				};
			}
		}
		#endregion
	}
}