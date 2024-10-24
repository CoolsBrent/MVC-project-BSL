using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MVC_Project_BSL.Data.UnitOfWork;
using MVC_Project_BSL.Models;
using MVC_Project_BSL.ViewModels;
using System.Diagnostics;

namespace MVC_Project_BSL.Controllers
{

    public class GroepsreisController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
		private readonly MonitorService _monitorService;

		public GroepsreisController(IUnitOfWork unitOfWork, MonitorService monitorService)
        {

            _unitOfWork = unitOfWork;
			_monitorService = monitorService;
		}

		// GET: Groepsreis
		public async Task<IActionResult> Index()
		{
			var actieveGroepsreizen = await _unitOfWork.GroepsreisRepository.GetAllAsync(
				query => query.Include(g => g.Bestemming).Where(g => !g.IsArchived));

			var gearchiveerdeGroepsreizen = await _unitOfWork.GroepsreisRepository.GetAllAsync(
				query => query.Include(g => g.Bestemming).Where(g => g.IsArchived));

			var viewModel = new GroepsreisViewModel
			{
				ActieveGroepsreizen = actieveGroepsreizen,
				GearchiveerdeGroepsreizen = gearchiveerdeGroepsreizen
			};

			return View(viewModel);
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
                groepsreis.Monitoren = groepsreis.Monitoren ?? new List<GroepsreisMonitor>();
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
            var groepsreis = await _unitOfWork.GroepsreisRepository.GetQueryable(
                query => query.Include(g => g.Onkosten))
                .FirstOrDefaultAsync(g => g.Id == id);

            if (groepsreis == null)
            {
                return NotFound();
            }

            ViewBag.Bestemmingen = new SelectList(
                await _unitOfWork.BestemmingRepository.GetAllAsync(),
                "Id",
                "BestemmingsNaam",
                groepsreis.BestemmingId);

            ViewBag.Activiteiten = new SelectList(
                await _unitOfWork.ActiviteitRepository.GetAllAsync(),
                "Id",
                "Naam",
                groepsreis.Activiteiten);

            return View(groepsreis);
        }

        private async Task<string> SaveFotoAsync(IFormFile fotoFile)
        {
            // Kies waar je de foto's wilt opslaan
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/onkosten");
            Directory.CreateDirectory(uploadsFolder);

            var uniekeBestandsnaam = Guid.NewGuid().ToString() + Path.GetExtension(fotoFile.FileName);
            var bestandspad = Path.Combine(uploadsFolder, uniekeBestandsnaam);

            using (var fileStream = new FileStream(bestandspad, FileMode.Create))
            {
                await fotoFile.CopyToAsync(fileStream);
            }

            // Retourneer het pad dat je wilt opslaan in de database (relatief pad)
            return "/uploads/onkosten/" + uniekeBestandsnaam;
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

            // Handmatige validatie voor FotoFile
            for (int i = 0; i < groepsreis.Onkosten.Count; i++)
            {
                var onkost = groepsreis.Onkosten[i];
                if (onkost.Id == 0 || string.IsNullOrEmpty(onkost.Foto))
                {
                    // Nieuwe onkost of onkost zonder bestaande foto
                    if (onkost.FotoFile == null || onkost.FotoFile.Length == 0)
                    {
                        ModelState.AddModelError($"Onkosten[{i}].FotoFile", "Het uploaden van een foto is verplicht.");
                    }
                }
            }

            if (!ModelState.IsValid)
            {
                // Log ModelState fouten
                foreach (var state in ModelState)
                {
                    foreach (var error in state.Value.Errors)
                    {
                        Debug.WriteLine($"ModelState Error in '{state.Key}': {error.ErrorMessage}");
                    }
                }

                // Laad ViewBags opnieuw
                ViewBag.Bestemmingen = new SelectList(
                    await _unitOfWork.BestemmingRepository.GetAllAsync(),
                    "Id",
                    "BestemmingsNaam",
                    groepsreis.BestemmingId);

                ViewBag.Activiteiten = new SelectList(
                    await _unitOfWork.ActiviteitRepository.GetAllAsync(),
                    "Id",
                    "Naam",
                    groepsreis.Activiteiten);

                return View(groepsreis);
            }

            try
            {
                // Log start van verwerking
                Debug.WriteLine("Begin verwerking van de Edit actie.");

                // Haal de bestaande groepsreis op inclusief onkosten
                var bestaandeGroepsreis = await _unitOfWork.GroepsreisRepository.GetQueryable(
                    query => query.Include(g => g.Onkosten))
                    .FirstOrDefaultAsync(g => g.Id == id);

                if (bestaandeGroepsreis == null)
                {
                    Debug.WriteLine("Bestaande groepsreis niet gevonden.");
                    return NotFound();
                }

                Debug.WriteLine("Bestaande groepsreis gevonden. Begin met updaten van basisgegevens.");

                // Update de basisgegevens
                bestaandeGroepsreis.Begindatum = groepsreis.Begindatum;
                bestaandeGroepsreis.Einddatum = groepsreis.Einddatum;
                bestaandeGroepsreis.Prijs = groepsreis.Prijs;
                bestaandeGroepsreis.BestemmingId = groepsreis.BestemmingId;

                Debug.WriteLine("Basisgegevens bijgewerkt.");

                // Zorg dat Onkosten lijsten niet null zijn
                groepsreis.Onkosten = groepsreis.Onkosten ?? new List<Onkosten>();
                bestaandeGroepsreis.Onkosten = bestaandeGroepsreis.Onkosten ?? new List<Onkosten>();

                // Verwijder bestaande onkosten die niet meer in de ingediende gegevens zitten
                var teVerwijderenOnkosten = bestaandeGroepsreis.Onkosten
                    .Where(o => !groepsreis.Onkosten.Any(ng => ng.Id == o.Id))
                    .ToList();

                Debug.WriteLine($"Aantal onkosten te verwijderen: {teVerwijderenOnkosten.Count}");

                foreach (var onkost in teVerwijderenOnkosten)
                {
                    bestaandeGroepsreis.Onkosten.Remove(onkost);
                    Debug.WriteLine($"Onkost met ID {onkost.Id} verwijderd.");
                }

                // Update of voeg onkosten toe
                for (int i = 0; i < groepsreis.Onkosten.Count; i++)
                {
                    var onkost = groepsreis.Onkosten[i];
                    var bestaandeOnkost = bestaandeGroepsreis.Onkosten.FirstOrDefault(o => o.Id == onkost.Id);

                    if (bestaandeOnkost != null)
                    {
                        Debug.WriteLine($"Update bestaande onkost met ID {onkost.Id}.");

                        // Update bestaande onkost
                        bestaandeOnkost.Titel = onkost.Titel;
                        bestaandeOnkost.Omschrijving = onkost.Omschrijving;
                        bestaandeOnkost.Bedrag = onkost.Bedrag;
                        bestaandeOnkost.Datum = onkost.Datum;

                        // Verwerk foto
                        if (onkost.FotoFile != null && onkost.FotoFile.Length > 0)
                        {
                            Debug.WriteLine("Nieuwe foto gevonden voor bestaande onkost. Foto opslaan.");
                            var fotoPad = await SaveFotoAsync(onkost.FotoFile);
                            bestaandeOnkost.Foto = fotoPad;
                        }
                        else
                        {
                            // Behoud de bestaande foto als er geen nieuwe is geüpload
                            bestaandeOnkost.Foto = onkost.Foto;
                        }
                    }
                    else
                    {
                        Debug.WriteLine("Voeg nieuwe onkost toe.");

                        // Voeg nieuwe onkost toe
                        var nieuweOnkost = new Onkosten
                        {
                            Titel = onkost.Titel,
                            Omschrijving = onkost.Omschrijving,
                            Bedrag = onkost.Bedrag,
                            Datum = onkost.Datum,
                            GroepsreisId = bestaandeGroepsreis.Id
                        };

                        // Verwerk foto
                        if (onkost.FotoFile != null && onkost.FotoFile.Length > 0)
                        {
                            Debug.WriteLine("Foto gevonden voor nieuwe onkost. Foto opslaan.");
                            var fotoPad = await SaveFotoAsync(onkost.FotoFile);
                            nieuweOnkost.Foto = fotoPad;
                        }

                        bestaandeGroepsreis.Onkosten.Add(nieuweOnkost);
                        Debug.WriteLine("Nieuwe onkost toegevoegd.");
                    }
                }

                _unitOfWork.GroepsreisRepository.Update(bestaandeGroepsreis);
                _unitOfWork.SaveChanges();

                Debug.WriteLine("Wijzigingen opgeslagen.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Fout opgetreden: {ex.Message}");
                throw;
            }

            return RedirectToAction(nameof(Index));
        }


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
                               .ThenInclude(m => m.Monitor.Persoon) // Persoon van Monitoren ophalen
                               .Include(g => g.Bestemming)
                               .ThenInclude(b => b.Fotos)
                               .Include(g => g.Deelnemers));

			// Zoek de specifieke groepsreis met het gegeven id
			var groepsreis = groepsreizen.FirstOrDefault(g => g.Id == id);

			var ingeschrevenMonitoren = groepsreis?.Monitoren.Select(m => m.Monitor.PersoonId).ToList();
			var uniekeMonitoren = monitoren
					.Where(m => !ingeschrevenMonitoren.Contains(m.PersoonId))
					.GroupBy(m => m.PersoonId)
					.Select(g => g.First())
					.ToList();

			var ingeschrevenDeelnemers = groepsreis?.Deelnemers.Select(m => m.KindId).ToList();
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

		// POST: Groepsreis/Archive/5
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Archive(int id)
		{
			var groepsreis = await _unitOfWork.GroepsreisRepository.GetByIdAsync(id);
			if (groepsreis == null)
			{
				return NotFound();
			}

			groepsreis.IsArchived = true;
			_unitOfWork.GroepsreisRepository.Update(groepsreis);
			_unitOfWork.SaveChanges();

			return RedirectToAction(nameof(Index));
		}

		// POST: Groepsreis/Activate/5
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Activate(int id)
		{
			var groepsreis = await _unitOfWork.GroepsreisRepository.GetByIdAsync(id);
			if (groepsreis == null)
			{
				return NotFound();
			}

			groepsreis.IsArchived = false;
			_unitOfWork.GroepsreisRepository.Update(groepsreis);
			_unitOfWork.SaveChanges();

			return RedirectToAction(nameof(Index));
		}


		//DEELNEMERS EN MONITOREN

		[HttpPost]
		public async Task<IActionResult> VoegDeelnemerToe(int groepsreisId, int kindId)
		{
			var groepsreis = await _unitOfWork.GroepsreisRepository.GetByIdAsync(groepsreisId);
			var kind = await _unitOfWork.KindRepository.GetByIdAsync(kindId);

            // Controleer of zowel de groepsreis als het kind bestaan
            if (groepsreis == null || kind == null)
            {
                return NotFound();
            }

            // Zorg ervoor dat Deelnemers niet null is
            if (groepsreis.Deelnemers == null)
            {
                groepsreis.Deelnemers = new List<Deelnemer>();
            }

            // Maak een nieuwe deelnemer aan en koppel deze aan het kind
            var deelnemer = new Deelnemer
            {
                KindId = kind.Id,
                GroepsreisDetailId = groepsreis.Id, // Zorg ervoor dat je de juiste property gebruikt
                Opmerkingen = "",
                Review = "",
                ReviewScore = 0,
            };

            // Voeg de deelnemer toe aan de groepsreis
            groepsreis.Deelnemers.Add(deelnemer);


            // Sla de wijzigingen op
            _unitOfWork.SaveChanges(); // Zorg ervoor dat je de async versie gebruikt

            // Redirect naar de detailpagina van de groepsreis
            return RedirectToAction("Detail", new { id = groepsreisId });
        }


        [HttpPost]
        public async Task<IActionResult> DeleteDeelnemer(int groepsreisId, int kindId)
        {
            Debug.WriteLine($"Verzoek ontvangen om kind met ID {kindId} te verwijderen uit groepsreis met ID {groepsreisId}.");

            // Groepsreis ophalen inclusief de deelnemers
            var groepsreis = await _unitOfWork.GroepsreisRepository.GetByIdWithIncludesAsync(groepsreisId, g => g.Deelnemers);

			if (groepsreis == null)
			{
				Debug.WriteLine($"Groepsreis met ID {groepsreisId} niet gevonden.");
				return NotFound();
			}

            // Log de deelnemers
            Debug.WriteLine($"Deelnemers in groepsreis {groepsreisId}: {string.Join(", ", groepsreis.Deelnemers.Select(d => d.KindId))}");

            // Zoek de deelnemer met het gegeven kindId
            var deelnemer = groepsreis.Deelnemers.FirstOrDefault(d => d.KindId == kindId);

            if (deelnemer == null)
            {
                Debug.WriteLine($"Kind met ID {kindId} is geen deelnemer aan groepsreis met ID {groepsreisId}.");
                return NotFound();
            }

            Debug.WriteLine($"Kind met ID {kindId} gevonden in groepsreis. Verwijderen...");
            groepsreis.Deelnemers.Remove(deelnemer);

            // Bevestig het verwijderen en sla wijzigingen op
            _unitOfWork.SaveChanges();
            Debug.WriteLine($"Kind met ID {kindId} succesvol verwijderd uit groepsreis met ID {groepsreisId}.");

			return RedirectToAction("Detail", new { id = groepsreisId });
		}


		[HttpPost]
		public async Task<IActionResult> MaakHoofdmonitor(int groepsreisId, int monitorId)
		{
			var result = await _monitorService.MaakHoofdmonitor(groepsreisId, monitorId);
			return result;
		}

		[HttpPost]
		public async Task<IActionResult> MaakGewoneMonitor(int groepsreisId, int monitorId)
		{
			var result = await _monitorService.MaakGewoneMonitor(groepsreisId, monitorId);
			return result;
		}
		[HttpPost]
        public async Task<IActionResult> DeleteMonitor(int groepsreisId, int monitorId)
        {
            Debug.WriteLine($"Verzoek ontvangen om monitor met ID {monitorId} te verwijderen uit groepsreis met ID {groepsreisId}.");

            // Groepsreis ophalen inclusief de ingeschreven monitoren
            var groepsreis = await _unitOfWork.GroepsreisRepository.GetByIdWithIncludesAsync(groepsreisId, g => g.Monitoren);
            var monitor = await _unitOfWork.MonitorRepository.GetByIdAsync(monitorId);

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
            foreach (var gm in groepsreis.Monitoren)
            {
                Debug.WriteLine($"Monitor ID: {gm.MonitorId}, Naam: {gm.Monitor?.Persoon?.Voornaam} {gm.Monitor?.Persoon?.Naam}");
            }

            // Zoek de specifieke GroepsreisMonitor die je wilt verwijderen
            var groepsreisMonitor = groepsreis.Monitoren.FirstOrDefault(gm => gm.MonitorId == monitorId);

            // Verwijder de monitor uit de groepsreis
            if (groepsreisMonitor != null)
            {
                Debug.WriteLine($"Monitor met ID {monitorId} gevonden in groepsreis. Verwijderen...");
                groepsreis.Monitoren.Remove(groepsreisMonitor);

                // Wijzigingen opslaan
                Debug.WriteLine("Wijzigingen opslaan...");
                _unitOfWork.SaveChanges(); // Gebruik de async versie
                Debug.WriteLine($"Monitor met ID {monitorId} succesvol verwijderd uit groepsreis met ID {groepsreisId}.");
            }
            else
            {
                Debug.WriteLine($"Monitor met ID {monitorId} is geen ingeschreven monitor in groepsreis met ID {groepsreisId}.");
            }

            return RedirectToAction("Detail", new { id = groepsreisId });
        }

        [HttpPost]
        public async Task<IActionResult> AddMonitor(int groepsreisId, int monitorId)
        {
            // Groepsreis ophalen inclusief de ingeschreven monitoren
            var groepsreis = await _unitOfWork.GroepsreisRepository.GetByIdWithIncludesAsync(groepsreisId, g => g.Monitoren);
            var monitor = await _unitOfWork.MonitorRepository.GetByIdAsync(monitorId);

            if (groepsreis == null || monitor == null)
            {
                return NotFound();
            }

            // Voeg de monitor toe aan de groepsreis
            if (!groepsreis.Monitoren.Any(m => m.Monitor.Id == monitor.Id))
            {
                var groepsreisMonitor = new GroepsreisMonitor
                {
                    GroepsreisId = groepsreis.Id,
                    MonitorId = monitor.Id
                };

                // Voeg de nieuwe GroepsreisMonitor toe aan de groepsreis
                groepsreis.Monitoren.Add(groepsreisMonitor);

                // Sla de wijzigingen op
                _unitOfWork.SaveChanges();
            }

            return RedirectToAction("Detail", new { id = groepsreisId });
        }


    }
}