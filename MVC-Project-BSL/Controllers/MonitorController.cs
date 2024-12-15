using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MVC_Project_BSL.Data.UnitOfWork;
using MVC_Project_BSL.Models;
using MVC_Project_BSL.Services;
using MVC_Project_BSL.ViewModels;
using System.Threading;
using static MVC_Project_BSL.ViewModels.MonitorViewModel;

namespace MVC_Project_BSL.Controllers
{
	public class MonitorController : Controller
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly MonitorService _monitorService;
		private readonly UserManager<CustomUser> _userManager;
		private readonly RoleManager<IdentityRole<int>> _roleManager;

		public MonitorController(IUnitOfWork unitOfWork, MonitorService monitorService, UserManager<CustomUser> userManager, RoleManager<IdentityRole<int>> roleManager)
		{
			_unitOfWork = unitOfWork;
			_monitorService = monitorService;
			_userManager = userManager;
			_roleManager = roleManager;
		}
		public async Task<IActionResult> Index()
		{
			var monitoren = await _unitOfWork.MonitorRepository.GetAllAsync(query =>
					query.Include(m => m.Persoon));

			

			var monitorViewModel = monitoren
				.Where(m => m.Persoon != null)
				.Select(m => new MonitorViewModel
				{
					Id = m.Id,
					PersoonId = m.PersoonId,
					Voornaam = m.Persoon.Voornaam,
					Naam = m.Persoon.Naam,
					IsHoofdMonitor = m.IsHoofdMonitor,
					Geboortedatum = m.Persoon.Geboortedatum,
					IsActief = m.Persoon.IsActief
				}).ToList();

			return View(monitorViewModel);
		}

		public async Task<IActionResult> Details(int id)
		{
            var monitoren = await _unitOfWork.MonitorRepository.GetAllAsync(query =>
      query.Include(m => m.Persoon) // Zorg ervoor dat Persoon wordt geladen
           .ThenInclude(p => p.Opleidingen)// Zorg ervoor dat Opleidingen van Persoon worden geladen
           .ThenInclude(o => o.Opleiding) 
           .Include(m => m.Groepsreizen) // Zorg ervoor dat Groepsreizen van Monitor worden geladen
           .ThenInclude(gr => gr.Groepsreis) // Zorg ervoor dat Groepsreis wordt geladen via Groepsreizen
           .ThenInclude(g => g.Bestemming)); // Zorg ervoor dat Bestemming van Groepsreis wordt geladen

            var monitor = monitoren.FirstOrDefault(m => m.Id == id);
			

            if (monitor == null)
			{
				return NotFound();
			}

			// Haal de CustomUser op (als deze bestaat)
			var customUser = monitor.Persoon;

			// Maak het MonitorViewModel
			var monitorViewModel = new MonitorViewModel
			{
				Id = monitor.Id,
				PersoonId = monitor.PersoonId,
				Voornaam = monitor.Persoon.Voornaam,
				Naam = monitor.Persoon.Naam,
				IsHoofdMonitor = monitor.IsHoofdMonitor,
				Geboortedatum = monitor.Persoon.Geboortedatum,

                // Map groepsreizen
                Groepsreizen = monitor.Groepsreizen?.Select(gr => new GroepsreisOverzichtViewModel
                {
                    Naam = gr.Groepsreis?.Bestemming?.BestemmingsNaam,  // Null-check voor Bestemming
                    Begindatum = gr.Groepsreis?.Begindatum ?? DateTime.MinValue,  // Null-check voor Begindatum
                    Einddatum = gr.Groepsreis?.Einddatum ?? DateTime.MinValue  // Null-check voor Einddatum
                }).ToList() ?? new List<GroepsreisOverzichtViewModel>(),

                // Map opleidingen via CustomUser
                Opleidingen = customUser?.Opleidingen?.Select(op => new OpleidingOverzichtViewModel
                {
                    Titel = op.Opleiding?.Naam ?? "Onbekend",  // Null-check voor Opleiding.Naam
                    BehaaldOp = op.Opleiding?.Einddatum ?? DateTime.MinValue  // Null-check voor Opleiding.Einddatum
                }).ToList() ?? new List<OpleidingOverzichtViewModel>()
            };

			return View(monitorViewModel);
		}
		[HttpPost]
		public async Task<IActionResult> DeactivateUser(int userId)
		{
			// Zoek de gebruiker op basis van de userId
			var user = await _userManager.FindByIdAsync(userId.ToString());

			if (user != null)
			{
				// Zet de IsActief status op false
				user.IsActief = false;

				var monitor = await _unitOfWork.MonitorRepository.GetByIdWithIncludesAsync(userId, m => m.Persoon);

				// Als er een monitor bestaat, werk deze bij
				if (monitor != null)
				{
					// Deactiveer de monitor

					// Maak het MonitorViewModel voor de weergave
					var monitorViewModel = new MonitorViewModel
					{
						PersoonId = monitor.PersoonId,
						Voornaam = monitor.Persoon.Voornaam,
						Naam = monitor.Persoon.Naam,
						IsHoofdMonitor = monitor.IsHoofdMonitor,
						Geboortedatum = monitor.Persoon.Geboortedatum,
						IsActief = monitor.Persoon.IsActief
					};

					// Werk de Monitor bij in de repository
					_unitOfWork.MonitorRepository.Update(monitor);
				}

				// Werk de gebruiker bij in de UserManager
				var result = await _userManager.UpdateAsync(user);
				if (!result.Succeeded)
				{
					// Als er een fout is, voeg een foutmelding toe
					ModelState.AddModelError("", "Er is een fout opgetreden bij het bijwerken van de gebruiker.");
					return View();  // Retourneer de huidige view met de foutmelding
				}

				// Sla de wijzigingen op in de database
				 _unitOfWork.SaveChanges();

				// Redirect naar de Index of een andere pagina
				return RedirectToAction("Index");
			}

			// Als de gebruiker niet gevonden is, geef een foutmelding weer
			ModelState.AddModelError("", "Gebruiker niet gevonden.");
			return View();  // Retourneer de huidige view met de foutmelding
		}
		public async Task<IActionResult> Edit(int id)
		{
			var user = await _unitOfWork.CustomUserRepository.GetByIdAsync(id);

			if (user == null)
			{
				return NotFound("Gebruiker niet gevonden.");
			}

			var viewModel = new PersoonlijkeGegevensViewModel
			{
				Naam = user.Naam,
				Voornaam = user.Voornaam,
				Geboortedatum = user.Geboortedatum,
				Huisdokter = user.Huisdokter,
				TelefoonNummer = user.TelefoonNummer,
				RekeningNummer = user.RekeningNummer,
				IsActief = user.IsActief,
			};

			return View("Edit", viewModel);
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(PersoonlijkeGegevensViewModel model)
		{
			if (!ModelState.IsValid)
			{
				return View("Edit", model);
			}

			var user = await _unitOfWork.CustomUserRepository.GetByIdAsync(model.Id);

			if (user == null)
			{
				return NotFound("Gebruiker niet gevonden.");
			}

			user.Id = model.Id;
			user.Naam = model.Naam;
			user.Voornaam = model.Voornaam;
			user.Geboortedatum = model.Geboortedatum;
			user.Huisdokter = model.Huisdokter;
			user.TelefoonNummer = model.TelefoonNummer;
			user.RekeningNummer = model.RekeningNummer;
			user.IsActief = model.IsActief;

			_unitOfWork.CustomUserRepository.Update(user);
			_unitOfWork.SaveChanges();

			TempData["SuccessMessage"] = "Gebruiker gegevens zijn correct opgeslagen!";
			return RedirectToAction(nameof(Index));
		}

		public async Task<IActionResult> Create()
		{
			// Haal alle gebruikers op
			var users = await _unitOfWork.CustomUserRepository.GetAllAsync();

			// Maak een lijst van gebruikers die de rol "Deelnemer" hebben
			var deelnemerUsers = new List<CustomUser>();

			foreach (var user in users)
			{
				var roles = await _userManager.GetRolesAsync(user);  // Haal de rollen van de gebruiker op

				if (roles.Contains("Deelnemer"))  // Controleer of de gebruiker de rol "Deelnemer" heeft
				{
					deelnemerUsers.Add(user);
				}
			}

			// Maak een lijst van gebruikers voor de view
			var viewModel = deelnemerUsers.Select(u => new CustomUser
			{
				Id = u.Id,
				Voornaam = u.Voornaam,
				Naam = u.Naam,
				Geboortedatum = u.Geboortedatum,
				IsActief = u.IsActief
			}).ToList();

			return View(viewModel);
		}




		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(MonitorViewModel model)
		{
			if (ModelState.IsValid)
			{
				var user = await _unitOfWork.CustomUserRepository.GetByIdAsync(model.PersoonId);
				if (user != null)
				{
					var monitor = new Models.Monitor
					{
						PersoonId = model.PersoonId,
						IsHoofdMonitor = model.IsHoofdMonitor,
						// Voeg hier andere velden toe
					};

					await _unitOfWork.MonitorRepository.AddAsync(monitor);
					_unitOfWork.SaveChanges();

					return RedirectToAction("Index");
				}
				ModelState.AddModelError("", "Gebruiker niet gevonden.");
			}
			return View(model);
		}
		[HttpPost]
		public async Task<IActionResult> MaakMonitor(int monitorId)
		{
			var redirectResult = await _monitorService.MaakMonitor(monitorId); // Service-aanroep
			if (redirectResult)
			{
				return RedirectToAction("Index", "Monitor"); // Redirect na bewerken
			}

			return View(); // Of een foutmelding als de redirect niet slaagt
		}
        [HttpPost]
        public async Task<IActionResult> MaakHoofdMonitor(int userId)
        {

            var redirectResult = await _monitorService.MaakHoofdMonitor(userId); // Service-aanroep
            if (redirectResult)
            {
                return RedirectToAction("Index", "Monitor"); // Redirect na bewerken
            }

            return View(); // Of een foutmelding als de redirect niet slaagt
        }
        [HttpPost]
        public async Task<IActionResult> MaakHoofdMonitorGewoneMonitor(int userId)
        {

            var redirectResult = await _monitorService.MaakHoofdMonitorGewoneMonitor(userId); // Service-aanroep
            if (redirectResult)
            {
                return RedirectToAction("Index", "Monitor"); // Redirect na bewerken
            }

            return View(); // Of een foutmelding als de redirect niet slaagt
        }


    }
}
