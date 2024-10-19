using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MVC_Project_BSL.Data.UnitOfWork;
using MVC_Project_BSL.Models;
using System.Diagnostics;

namespace MVC_Project_BSL.Controllers
{

	public class GroepsreisController : Controller
	{
		private readonly IUnitOfWork _unitOfWork;

		public GroepsreisController(IUnitOfWork unitOfWork)
		{

			_unitOfWork = unitOfWork;
		}

		// GET: Groepsreis
		public async Task<IActionResult> Index()
		{
			var groepsreizen = await _unitOfWork.GroepsreisRepository.GetAllAsync(query => query.Include(g => g.Bestemming));
			return View(groepsreizen);
		}

		// GET: Groepsreis/Create
		public IActionResult Create()
		{
			// Zorg ervoor dat je de bestemmingen ophaalt vanuit de Bestemming repository of DbSet
			ViewBag.Bestemmingen = new SelectList(_unitOfWork.BestemmingRepository.GetAllAsync().Result, "Id", "BestemmingsNaam");
			ViewBag.Activiteiten = new SelectList(_unitOfWork.ActiviteitRepository.GetAllAsync().Result, "Id", "Naam");

			return View();
		}

		// POST: Groepsreis/Create
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(Groepsreis groepsreis)
		{
			// Log elke validatiefout
			foreach (var modelState in ModelState)
			{
				foreach (var error in modelState.Value.Errors)
				{
					Console.WriteLine($"Fout in {modelState.Key}: {error.ErrorMessage}");
				}
			}

			if (ModelState.IsValid)
			{

				// Zorg ervoor dat de collecties altijd zijn geïnitialiseerd om fouten te voorkomen
				groepsreis.Deelnemers = groepsreis.Deelnemers ?? new List<Deelnemer>();
				groepsreis.Monitoren = groepsreis.Monitoren ?? new List<Models.Monitor>();
				groepsreis.Onkosten = groepsreis.Onkosten ?? new List<Onkosten>();
				groepsreis.Programmas = groepsreis.Programmas ?? new List<Programma>();


				// Sla de nieuwe groepsreis op als het formulier geldig is
				await _unitOfWork.GroepsreisRepository.AddAsync(groepsreis);

				_unitOfWork.SaveChanges();

				return RedirectToAction(nameof(Index));
			}

			// Als het formulier niet geldig is, moet je de bestemmingen opnieuw in de ViewBag laden
			ViewBag.Bestemmingen = new SelectList(await _unitOfWork.BestemmingRepository.GetAllAsync(), "Id", "BestemmingsNaam", groepsreis.BestemmingId);
			ViewBag.Activiteiten = new SelectList(await _unitOfWork.ActiviteitRepository.GetAllAsync(), "Id", "Naam");

			// Geef het formulier opnieuw weer met de ingevulde gegevens
			return View(groepsreis);
		}

		// GET: Groepsreis/Edit/5
		public async Task<IActionResult> Edit(int id)
		{
			var groepsreis = await _unitOfWork.GroepsreisRepository.GetByIdAsync(id);
			if (groepsreis == null)
			{
				return NotFound();
			}
			ViewBag.Bestemmingen = new SelectList(await _unitOfWork.BestemmingRepository.GetAllAsync(), "Id", "BestemmingsNaam", groepsreis.BestemmingId);

			ViewBag.Activiteiten = new SelectList(await _unitOfWork.ActiviteitRepository.GetAllAsync(), "Id", "Naam", groepsreis.Programmas);
			return View(groepsreis);
		}

		// POST: Groepsreis/Edit/5
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, Groepsreis groepsreis)
		{
			if (id != groepsreis.Id)
			{
				return NotFound();
			}

			if (ModelState.IsValid)
			{
				try
				{
					_unitOfWork.GroepsreisRepository.Update(groepsreis);
					_unitOfWork.SaveChanges();
				}
				catch (DbUpdateConcurrencyException)
				{
					if (!await GroepsreisExists(groepsreis.Id))
					{
						return NotFound();
					}
					else
					{
						throw;
					}
				}
				return RedirectToAction(nameof(Index));
			}
			ViewBag.Bestemmingen = new SelectList(await _unitOfWork.BestemmingRepository.GetAllAsync(), "Id", "BestemmingsNaam", groepsreis.BestemmingId);

			ViewBag.Activiteiten = new SelectList(await _unitOfWork.ActiviteitRepository.GetAllAsync(), "Id", "Naam", groepsreis.Programmas);
			return View(groepsreis);
		}

		// GET: Groepsreis/Delete/5
		public async Task<IActionResult> Delete(int id)
		{
			var groepsreis = await _unitOfWork.GroepsreisRepository.GetQueryable(
				query => query.Include(g => g.Bestemming))
				.FirstOrDefaultAsync(g => g.Id == id);
			if (groepsreis == null)
			{
				return NotFound();
			}
			return View(groepsreis);
		}

		// POST: Groepsreis/Delete/5
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			var groepsreis = await _unitOfWork.GroepsreisRepository.GetByIdAsync(id);
			if (groepsreis != null)
			{
				_unitOfWork.GroepsreisRepository.Delete(groepsreis);
				_unitOfWork.SaveChanges();
			}
			return RedirectToAction(nameof(Index));
		}

		private async Task<bool> GroepsreisExists(int id)
		{
			var groepsreis = await _unitOfWork.GroepsreisRepository.GetByIdAsync(id);
			return groepsreis != null;
		}
		// GET: Groepsreis/Detail/5
		public async Task<IActionResult> Detail(int id)
		{

			var monitoren = await _unitOfWork.MonitorRepository.GetAllAsync(
				query => query.Include(m => m.Persoon));

			var deelnemers = await _unitOfWork.KindRepository.GetAllAsync(
				query => query.Include(m => m.Persoon));


			// Haal alle groepsreizen op met de bijbehorende monitoren en hun personen
			var groepsreizen = await _unitOfWork.GroepsreisRepository.GetAllAsync(
				query => query.Include(g => g.Monitoren)
							   .ThenInclude(m => m.Persoon) // Persoon van Monitoren ophalen
							   .Include(g => g.Bestemming)
							   .ThenInclude(b => b.Fotos)
							   .Include(g => g.Deelnemers));

			// Zoek de specifieke groepsreis met het gegeven id
			var groepsreis = groepsreizen.FirstOrDefault(g => g.Id == id);

			var ingeschrevenMonitoren = groepsreis?.Monitoren.Select(m => m.PersoonId).ToList();
			var uniekeMonitoren = monitoren
					.Where(m => !ingeschrevenMonitoren.Contains(m.PersoonId))
					.GroupBy(m => m.PersoonId)
					.Select(g => g.First())
					.ToList();

			var ingeschrevenDeelnemers = groepsreis?.Deelnemers.Select(m => m.Id).ToList();
			var uniekeDeelnemers = deelnemers
					.Where(m => !ingeschrevenDeelnemers.Contains(m.Id))  // Selecteer de eerste unieke deelnemer per groep (PersoonId)
					.ToList();

			groepsreis.BeschikbareMonitoren = uniekeMonitoren.ToList();
			groepsreis.BeschikbareDeelnemers = uniekeDeelnemers.ToList();

			if (groepsreis == null)
			{
				return NotFound();
			}


			return View(groepsreis);
		}

        [HttpPost]
        public async Task<IActionResult> VoegDeelnemerToe(int groepsreisId, int kindId)
        {
            // Haal de groepsreis op
            var groepsreis = await _unitOfWork.GroepsreisRepository.GetByIdAsync(groepsreisId);

            // Haal het kind op
            var kind = await _unitOfWork.KindRepository.GetByIdAsync(kindId);

            // Controleer of zowel de groepsreis als het kind bestaan
            if (groepsreis == null || kind == null)
            {
                return NotFound();
            }

            // Maak een nieuwe deelnemer aan en koppel deze aan het kind
            var deelnemer = new Deelnemer
            {
                KindId = kind.Id,
                GroepsreisDetailId = groepsreis.Id // Zorg ervoor dat je de juiste property gebruikt
            };

            // Voeg de deelnemer toe aan de groepsreis
            groepsreis.Deelnemers.Add(deelnemer);
            // Verwijder het kind uit de Kind-tabel
            _unitOfWork.KindRepository.Delete(kind);

            // Sla de wijzigingen op
            _unitOfWork.SaveChanges();

            // Redirect naar de detailpagina van de groepsreis
            return RedirectToAction("Detail", new { id = groepsreisId });
        }
        [HttpPost]
        public async Task<IActionResult> DeleteDeelnemer(int groepsreisId, int kindId)
        {
            Debug.WriteLine($"Verzoek ontvangen om kind met ID {kindId} te verwijderen uit groepsreis met ID {groepsreisId}.");

            // Groepsreis ophalen inclusief de deelnemers
            var groepsreis = await _unitOfWork.GroepsreisRepository.GetByIdWithIncludesAsync(
                groepsreisId, g => g.Deelnemers);

            // Controleer of de groepsreis bestaat
            if (groepsreis == null)
            {
                Debug.WriteLine($"Groepsreis met ID {groepsreisId} niet gevonden.");
                return NotFound();
            }

            // Zoek de deelnemer met het gegeven kindId
            var deelnemer = groepsreis.Deelnemers.FirstOrDefault(d => d.KindId == kindId);

            // Controleer of de deelnemer bestaat
            if (deelnemer == null)
            {
                Debug.WriteLine($"Kind met ID {kindId} is geen deelnemer aan groepsreis met ID {groepsreisId}.");
                return NotFound();
            }

            // Verwijder de deelnemer uit de groepsreis
            Debug.WriteLine($"Kind met ID {kindId} gevonden in groepsreis. Verwijderen...");
            groepsreis.Deelnemers.Remove(deelnemer);

            var kind = await _unitOfWork.KindRepository.GetByIdAsync(kindId);
            if (kind == null)
            {
                Debug.WriteLine($"Kind met ID {kindId} niet gevonden.");
                return NotFound();
            }
            await _unitOfWork.KindRepository.AddAsync(kind);

            // Wijzigingen opslaan
            _unitOfWork.SaveChanges();
            Debug.WriteLine($"Kind met ID {kindId} succesvol verwijderd uit groepsreis met ID {groepsreisId}.");

            return RedirectToAction("Detail", new { id = groepsreisId });
        }

        [HttpPost]
		public async Task<IActionResult> MaakHoofdmonitor(int groepsreisId, string monitorId)
		{
			var groepsreis = await _unitOfWork.GroepsreisRepository.GetByIdWithIncludesAsync(groepsreisId, g => g.Monitoren);

			if (groepsreis == null)
			{
				return NotFound("Groepsreis niet gevonden");
			}

			foreach (var monitor in groepsreis.Monitoren)
			{
				monitor.IsHoofdMonitor = false;
			}

			var geselecteerdeMonitor = await _unitOfWork.MonitorRepository.GetByStringIdAsync(monitorId);
			if (geselecteerdeMonitor == null)
			{
				return NotFound("Monitor niet gevonden");
			}

			geselecteerdeMonitor.IsHoofdMonitor = true;

			_unitOfWork.SaveChanges();

			return RedirectToAction("Detail", new { id = groepsreisId });
		}
		[HttpPost]
		public async Task<IActionResult> MaakGewoneMonitor(int groepsreisId, string monitorId)
		{
			var groepsreis = await _unitOfWork.GroepsreisRepository.GetByIdWithIncludesAsync(groepsreisId, g => g.Monitoren);

			if (groepsreis == null)
			{
				return NotFound("Groepsreis niet gevonden");
			}

			var geselecteerdeMonitor = await _unitOfWork.MonitorRepository.GetByStringIdAsync(monitorId);
			if (geselecteerdeMonitor == null)
			{
				return NotFound("Monitor niet gevonden");
			}

			// Zet de hoofdmonitor-status terug naar "niet hoofdmonitor"
			geselecteerdeMonitor.IsHoofdMonitor = false;

			_unitOfWork.SaveChanges();

			return RedirectToAction("Detail", new { id = groepsreisId });
		}
		[HttpPost]
		public async Task<IActionResult> DeleteMonitor(int groepsreisId, string monitorId)
		{
			Debug.WriteLine($"Verzoek ontvangen om monitor met ID {monitorId} te verwijderen uit groepsreis met ID {groepsreisId}.");

			// Groepsreis ophalen inclusief de ingeschreven monitoren
			var groepsreis = await _unitOfWork.GroepsreisRepository.GetByIdWithIncludesAsync(groepsreisId, g => g.Monitoren);
			var monitor = await _unitOfWork.MonitorRepository.GetByStringIdAsync(monitorId);

			if (groepsreis == null)
			{
				Debug.WriteLine($"Groepsreis met ID {groepsreisId} niet gevonden.");
				return NotFound();
			}

			if (monitor == null)
			{
				Debug.WriteLine($"Monitor met ID {monitorId} niet gevonden.");
				return NotFound();
			}

			// Monitoren loggen die ingeschreven zijn in de groepsreis
			Debug.WriteLine("Huidige ingeschreven monitoren in groepsreis:");
			foreach (var m in groepsreis.Monitoren)
			{
				Debug.WriteLine($"Monitor ID: {m.Id}, Naam: {m.Persoon?.Voornaam} {m.Persoon?.Naam}");
			}

			// Verwijder de monitor uit de groepsreis
			if (groepsreis.Monitoren.Contains(monitor))
			{
				Debug.WriteLine($"Monitor met ID {monitorId} gevonden in groepsreis. Verwijderen...");
				groepsreis.Monitoren.Remove(monitor);



				// Wijzigingen opslaan
				Debug.WriteLine("Wijzigingen opslaan...");
				_unitOfWork.SaveChanges();
				Debug.WriteLine($"Monitor met ID {monitorId} succesvol verwijderd uit groepsreis met ID {groepsreisId}.");
			}
			else
			{
				Debug.WriteLine($"Monitor met ID {monitorId} is geen ingeschreven monitor in groepsreis met ID {groepsreisId}.");
			}

			return RedirectToAction("Detail", new { id = groepsreisId });
		}
		[HttpPost]
		public async Task<IActionResult> AddMonitor(int groepsreisId, string monitorId)
		{
			Debug.WriteLine($"Verzoek ontvangen om monitor met ID {monitorId} toe te voegen aan groepsreis met ID {groepsreisId}.");

			// Groepsreis ophalen inclusief de ingeschreven monitoren
			var groepsreis = await _unitOfWork.GroepsreisRepository.GetByIdWithIncludesAsync(groepsreisId, g => g.Monitoren);
			var monitor = await _unitOfWork.MonitorRepository.GetByStringIdAsync(monitorId);

			if (groepsreis == null)
			{
				Debug.WriteLine($"Groepsreis met ID {groepsreisId} niet gevonden.");
				return NotFound();
			}

			if (monitor == null)
			{
				Debug.WriteLine($"Monitor met ID {monitorId} niet gevonden.");
				return NotFound();
			}

			// Voeg de monitor toe aan de groepsreis
			if (!groepsreis.Monitoren.Contains(monitor))
			{
				Debug.WriteLine($"Monitor met ID {monitorId} is nog niet ingeschreven in de groepsreis. Toevoegen...");
				groepsreis.Monitoren.Add(monitor);


				// Wijzigingen opslaan
				_unitOfWork.SaveChanges();
				Debug.WriteLine($"Monitor met ID {monitorId} succesvol toegevoegd aan groepsreis met ID {groepsreisId}.");
			}
			else
			{
				Debug.WriteLine($"Monitor met ID {monitorId} is al ingeschreven in groepsreis met ID {groepsreisId}.");
			}

			return RedirectToAction("Detail", new { id = groepsreisId });
		}

	}
}

