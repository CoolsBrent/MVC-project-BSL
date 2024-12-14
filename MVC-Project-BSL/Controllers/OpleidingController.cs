using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MVC_Project_BSL.Data.UnitOfWork;
using MVC_Project_BSL.Models;
using System.Diagnostics;
using System.Security.Claims;

namespace MVC_Project_BSL.Controllers
{
    public class OpleidingController : Controller
    {
        #region Fields and Constructor
        private readonly IUnitOfWork _unitOfWork;

        public OpleidingController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        #endregion

        #region Index and Details Actions

        // GET: Opleiding
        public async Task<IActionResult> Index()
        {
            var opleidingen = await _unitOfWork.OpleidingRepository.GetAllAsync(
                query => query.Include(o => o.OpleidingPersonen));
            foreach (var opleiding in opleidingen)
            {
                opleiding.IngeschrevenPersonen = opleiding.OpleidingPersonen.Count;
            }
            return View(opleidingen);
        }

		// GET: Opleiding/Details/5
		public async Task<IActionResult> Details(int id)
		{
			// Haal de opleiding op met de benodigde relaties
			var opleiding = await _unitOfWork.OpleidingRepository.GetQueryable(
				query => query.Include(o => o.OpleidingPersonen)
							  .ThenInclude(op => op.Persoon) // Voeg de Persoon toe aan OpleidingPersonen
							  .Include(o => o.OpleidingVereist))
				.FirstOrDefaultAsync(o => o.Id == id);

			if (opleiding == null)
			{
				return NotFound();
			}
			var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
			var user = await _unitOfWork.CustomUserRepository.GetByIdAsync(userId);

			bool isIngeschreven = false;
			
				// Controleer of de gebruiker al ingeschreven is
				isIngeschreven = opleiding.OpleidingPersonen
					.Any(op => op.PersoonId == userId);

			ViewData["IsIngeschreven"] = isIngeschreven;
			// Check of de gebruiker de vereiste opleiding heeft afgerond
			if (opleiding.OpleidingVereistId.HasValue)
			{
				
				if (opleiding != null)
				{
					var heeftAfgerond = opleiding.OpleidingPersonen
						.Any(op => op.OpleidingId == opleiding.Id && op.Opleiding.OpleidingVereist.Einddatum < DateTime.Now);

					ViewData["HeeftVereisteOpleidingAfgerond"] = heeftAfgerond;
				}
			}
			

			opleiding!.IngeschrevenPersonen = opleiding.OpleidingPersonen.Count;

			
			// Haal alle actieve monitoren op
			var monitoren = await _unitOfWork.MonitorRepository.GetAllAsync(
				query => query.Include(m => m.Persoon).Where(m => m.Persoon.IsActief));

			// Filter monitoren die al aan de opleiding gekoppeld zijn
			var beschikbareMonitoren = monitoren
				.Where(m => !opleiding.OpleidingPersonen.Any(op => op.PersoonId == m.PersoonId))
				.ToList();

			// Voeg de beschikbare monitoren toe aan de opleiding
			opleiding.BeschikbareMonitoren = beschikbareMonitoren;

			// Geef de opleiding door aan de view
			return View(opleiding);
		}



		[HttpPost]
		public async Task<IActionResult> AddMonitor(int opleidingId, int monitorId)
		{
			// Zoek de opleiding op basis van opleidingId
			var opleiding = await _unitOfWork.OpleidingRepository.GetByIdAsync(opleidingId);
			if (opleiding == null)
			{
				return NotFound(); // Als opleiding niet gevonden is
			}

			// Zoek de monitor (persoon) op basis van monitorId
			var monitor = await _unitOfWork.MonitorRepository.GetByIdAsync(monitorId);
			if (monitor == null)
			{
				return NotFound(); // Als monitor niet gevonden is
			}

			// Voeg de monitor toe aan de opleiding zonder de gegevens van de monitor te overschrijven
			var opleidingPersoon = new OpleidingPersoon
			{
				OpleidingId = opleidingId,
				PersoonId = monitor.PersoonId // Dit is de relatie tussen opleiding en persoon
			};

			// Voeg de nieuwe relatie toe aan de Opleiding
			opleiding.OpleidingPersonen.Add(opleidingPersoon);

			// Sla de veranderingen op in de database
			_unitOfWork.SaveChanges();

			// Redirect naar de details van de opleiding
			return RedirectToAction(nameof(Details), new { id = opleidingId });
		}

		[HttpPost]
		public async Task<IActionResult> DeleteMonitor(int opleidingId, int monitorId)
		{
			Debug.WriteLine($"Verzoek ontvangen om monitor met ID {monitorId} te verwijderen uit opleiding met ID {opleidingId}.");

			// Haal de opleiding op inclusief de ingeschreven monitoren
			// Haal de opleiding inclusief personen op
			var opleiding = await _unitOfWork.OpleidingRepository.GetQueryable(
				query => query.Include(o => o.OpleidingPersonen)
							  .ThenInclude(op => op.Persoon))
				.FirstOrDefaultAsync(o => o.Id == opleidingId);

			var monitor = await _unitOfWork.CustomUserRepository.GetByIdAsync(monitorId);

			if (opleiding == null)
			{
				Debug.WriteLine($"Opleiding met ID {opleidingId} niet gevonden.");
				return NotFound();
			}

			if (monitor == null)
			{
				Debug.WriteLine($"Monitor met ID {monitorId} niet gevonden.");
				return NotFound();
			}

			// Log de huidige monitoren die zijn ingeschreven in de opleiding
			Debug.WriteLine("Huidige ingeschreven monitoren in opleiding:");
			foreach (var op in opleiding.OpleidingPersonen)
			{
				Debug.WriteLine($"Monitor ID: {op.PersoonId}, Naam: {op.Persoon?.Voornaam} {op.Persoon?.Naam}");
			}

			// Zoek de specifieke relatie in OpleidingPersonen die je wilt verwijderen
			var opleidingPersoon = opleiding.OpleidingPersonen.FirstOrDefault(op => op.PersoonId == monitorId);

			// Verwijder de monitor uit de opleiding
			if (opleidingPersoon != null)
			{
				Debug.WriteLine($"Monitor met ID {monitorId} gevonden in opleiding. Verwijderen...");
				opleiding.OpleidingPersonen.Remove(opleidingPersoon);

				// Wijzigingen opslaan
				Debug.WriteLine("Wijzigingen opslaan...");
				_unitOfWork.SaveChanges();
				Debug.WriteLine($"Monitor met ID {monitorId} succesvol verwijderd uit opleiding met ID {opleidingId}.");
			}
			else
			{
				Debug.WriteLine($"Monitor met ID {monitorId} is geen ingeschreven monitor in opleiding met ID {opleidingId}.");
			}

			return RedirectToAction("Details", new { id = opleidingId });
		}




		#endregion

		#region Create Actions
		[HttpGet]
        public async Task<IActionResult> Create()
        {
            var opleidingen = await _unitOfWork.OpleidingRepository.GetAllAsync();
            ViewBag.Opleidingen = opleidingen
                .Select(o => new SelectListItem
                {
                    Value = o.Id.ToString(),
                    Text = o.Naam
                }).ToList();

            // Voeg een optie toe voor "Geen"
            ViewBag.Opleidingen.Insert(0, new SelectListItem { Value = "", Text = "Geen" });

            return View();
        }

        // POST: Opleiding/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Opleiding opleiding)
        {
            LogModelStateErrors();

            if (ModelState.IsValid)
            {
                // Prevent cyclische vereisten
                if (IsCyclicPrerequisite(opleiding.Id, opleiding.OpleidingVereistId))
                {
                    ModelState.AddModelError("OpleidingVereistId", "Cylische vereiste opleiding gedetecteerd.");
                }
                else
                {
                    await _unitOfWork.OpleidingRepository.AddAsync(opleiding);
                    _unitOfWork.SaveChanges();

                    return RedirectToAction(nameof(Index));
                }
            }

            // Als ModelState niet geldig is, laad de opleidingen opnieuw
            var opleidingenList = await _unitOfWork.OpleidingRepository.GetAllAsync();
            ViewBag.Opleidingen = opleidingenList
                .Select(o => new SelectListItem
                {
                    Value = o.Id.ToString(),
                    Text = o.Naam
                }).ToList();

            ViewBag.Opleidingen.Insert(0, new SelectListItem { Value = "", Text = "Geen" });

            return View(opleiding);
        }

        private bool IsCyclicPrerequisite(int opleidingId, int? vereisteOpleidingId)
        {
            if (!vereisteOpleidingId.HasValue)
                return false;

            if (opleidingId == vereisteOpleidingId.Value)
                return true;

            var vereisteOpleiding = _unitOfWork.OpleidingRepository.GetByIdAsync(vereisteOpleidingId.Value).Result;
            if (vereisteOpleiding == null)
                return false;

            return IsCyclicPrerequisite(opleidingId, vereisteOpleiding.OpleidingVereistId);
        }

		#endregion

		#region Edit Actions

		// GET: Opleiding/Edit/5
		public async Task<IActionResult> Edit(int id)
		{
			var opleiding = await _unitOfWork.OpleidingRepository.GetQueryable(
				query => query.Include(o => o.OpleidingPersonen)
							  .Include(o => o.OpleidingVereist)) // Voeg meer includes toe als nodig
				.FirstOrDefaultAsync(o => o.Id == id);

			if (opleiding == null)
			{
				return NotFound();
			}

			var opleidingen = await _unitOfWork.OpleidingRepository.GetAllAsync();
			ViewBag.Opleidingen = opleidingen
				.Where(o => o.Id != id) // Vermijd zelfreferentie
				.Select(o => new SelectListItem
				{
					Value = o.Id.ToString(),
					Text = o.Naam
				}).ToList();

			ViewBag.Opleidingen.Insert(0, new SelectListItem { Value = "", Text = "Geen" });

			return View(opleiding);
		}


		// POST: Opleiding/Edit/5
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, Opleiding opleiding)
		{
			if (id != opleiding.Id)
			{
				return NotFound();
			}

			if (ModelState.IsValid)
			{
				try
				{
					// Controleer op cyclische vereisten
					if (IsCyclicPrerequisite(opleiding.Id, opleiding.OpleidingVereistId))
					{
						ModelState.AddModelError("OpleidingVereistId", "Cyclische vereiste opleiding gedetecteerd.");
					}
					else
					{
						// Update de opleiding
						var existingOpleiding = await _unitOfWork.OpleidingRepository.GetQueryable(
							query => query.Include(o => o.OpleidingPersonen))
							.FirstOrDefaultAsync(o => o.Id == id);

						if (existingOpleiding == null)
						{
							return NotFound();
						}

						// Update eigenschappen handmatig indien nodig
						existingOpleiding.Naam = opleiding.Naam;
						existingOpleiding.Beschrijving = opleiding.Beschrijving;
						existingOpleiding.Begindatum = opleiding.Begindatum;
						existingOpleiding.Einddatum = opleiding.Einddatum;
						existingOpleiding.AantalPlaatsen = opleiding.AantalPlaatsen;
						existingOpleiding.OpleidingVereistId = opleiding.OpleidingVereistId;

						_unitOfWork.OpleidingRepository.Update(existingOpleiding);
						_unitOfWork.SaveChanges();

						return RedirectToAction(nameof(Index));
					}
				}
				catch (DbUpdateConcurrencyException)
				{
					if (!await OpleidingExists(opleiding.Id))
					{
						return NotFound();
					}
					else
					{
						throw;
					}
				}
			}

			// Herladen van opleidingenlijst bij validatiefouten
			var opleidingen = await _unitOfWork.OpleidingRepository.GetAllAsync();
			ViewBag.Opleidingen = opleidingen
				.Where(o => o.Id != id) // Vermijd zelfreferentie
				.Select(o => new SelectListItem
				{
					Value = o.Id.ToString(),
					Text = o.Naam
				}).ToList();

			ViewBag.Opleidingen.Insert(0, new SelectListItem { Value = "", Text = "Geen" });

			return View(opleiding);
		}


		#endregion

		#region Delete Actions

		// GET: Opleiding/Delete/5
		public async Task<IActionResult> Delete(int id)
        {
            var opleiding = await _unitOfWork.OpleidingRepository.GetQueryable(
                query => query.Include(o => o.OpleidingPersonen)
                              .Include(o => o.OpleidingenAfhankelijk))
                .FirstOrDefaultAsync(o => o.Id == id);

            if (opleiding == null)
            {
                return NotFound();
            }

            // Controleer of deze opleiding als vereiste wordt gebruikt
            bool isPrerequisite = await _unitOfWork.OpleidingRepository.AnyAsync(o => o.OpleidingVereistId == id);
            var afhankelijkeOpleidingen = new List<Opleiding>();

            if (isPrerequisite)
            {
                afhankelijkeOpleidingen = (await _unitOfWork.OpleidingRepository.GetAllAsync(query => query.Where(o => o.OpleidingVereistId == id))).ToList();
            }

            ViewBag.IsPrerequisite = isPrerequisite;
            ViewBag.AfhankelijkeOpleidingen = afhankelijkeOpleidingen;

            return View(opleiding);
        }

        // POST: Opleiding/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var opleiding = await _unitOfWork.OpleidingRepository.GetByIdAsync(id);
            if (opleiding == null)
            {
                return NotFound();
            }

            // Controleer of deze opleiding als vereiste wordt gebruikt
            bool isPrerequisite = await _unitOfWork.OpleidingRepository.AnyAsync(o => o.OpleidingVereistId == id);

            if (isPrerequisite)
            {
                // Voeg een modelstate-fout toe
                ModelState.AddModelError("", "Deze opleiding kan niet worden verwijderd omdat deze als vereiste wordt gebruikt door andere opleidingen.");

                // Laad de opleiding en gerelateerde data opnieuw voor de view
                var afhankelijkeOpleidingen = await _unitOfWork.OpleidingRepository.GetAllAsync(query => query.Where(o => o.OpleidingVereistId == id));
                ViewBag.IsPrerequisite = isPrerequisite;
                ViewBag.AfhankelijkeOpleidingen = afhankelijkeOpleidingen;

                return View("Delete", opleiding);
            }

            _unitOfWork.OpleidingRepository.Delete(opleiding);
            try
            {
                _unitOfWork.SaveChanges();
            }
            catch (DbUpdateException ex)
            {
				// Log de fout indien nodig en toon een algemene foutmelding
				Debug.Print($"Fout bij het verwijderen van opleiding: {ex.Message}");
				ModelState.AddModelError("", "Kan de opleiding niet verwijderen. Probeer het opnieuw of neem contact op met de beheerder.");
                return View("Delete", opleiding);
            }

            return RedirectToAction(nameof(Index));
        }

        #endregion

        #region Helper Methods

        private void LogModelStateErrors()
        {
            foreach (var modelState in ModelState)
            {
                foreach (var error in modelState.Value.Errors)
                {
                    Debug.WriteLine($"Fout in {modelState.Key}: {error.ErrorMessage}");
                }
            }
        }

        private async Task<bool> OpleidingExists(int id)
        {
            return await _unitOfWork.OpleidingRepository.AnyAsync(o => o.Id == id);
        }

        [HttpGet]
        public async Task<JsonResult> GetOpleidingen(string term)
        {
            var opleidingen = await _unitOfWork.OpleidingRepository.GetAllAsync();

            if (!string.IsNullOrWhiteSpace(term))
            {
                opleidingen = opleidingen
                    .Where(o => o.Naam.Contains(term, StringComparison.OrdinalIgnoreCase))
                    .Take(10)
                    .ToList();
            }

            return Json(opleidingen.Select(o => o.Naam).ToList());
        }

        #endregion

        #region Subscribe Actions

        [HttpPost]
		public async Task<IActionResult> Inschrijven(int opleidingId)
		{
			var opleiding = await _unitOfWork.OpleidingRepository.GetByIdAsync(opleidingId);
			

			if (opleiding == null)
			{
				return NotFound();
			}

			var user = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

			if (user == 0)
			{
				return NotFound("Gebruiker niet gevonden.");
			}
			// Controleer of de opleiding een vereiste opleiding heeft
			if (opleiding.OpleidingVereistId.HasValue)
			{

				if (opleiding != null)
				{
					var heeftAfgerond = opleiding.OpleidingPersonen
						.Any(op => op.OpleidingId == opleiding.Id && op.Opleiding.Einddatum < DateTime.Now);

					if (!heeftAfgerond)
					{
						// Als de gebruiker de vereiste opleiding niet heeft afgerond, toon een foutmelding
						return BadRequest("Je moet eerst de vereiste opleiding hebben afgerond voordat je je kunt inschrijven voor deze opleiding.");
					}
				}
			}

		

			opleiding!.IngeschrevenPersonen = opleiding.OpleidingPersonen.Count;
			// Voeg de gebruiker toe aan de opleiding
			opleiding.OpleidingPersonen.Add(new OpleidingPersoon
			{
				OpleidingId = opleiding.Id,
				PersoonId = user // Zorg ervoor dat je de juiste relatie hebt
			});

			_unitOfWork.SaveChanges();

			return RedirectToAction(nameof(Details), new { id = opleiding.Id });
		}

		[HttpPost]
		public async Task<IActionResult> Uitschrijven(int opleidingId)
		{
			// Haal de opleiding op, inclusief de OpleidingPersonen (ingeschreven gebruikers)
			var opleiding = await _unitOfWork.OpleidingRepository
				.GetByIdWithIncludesAsync(opleidingId, o => o.OpleidingPersonen);

			if (opleiding == null)
			{
				return NotFound();
			}
			var user = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

			if (user == 0)
			{
				return NotFound("Gebruiker niet gevonden.");
			}

			// Zoek de OpleidingPersoon die de gebruiker aan deze opleiding koppelt
			var opleidingPersoon = opleiding.OpleidingPersonen
				.FirstOrDefault(op => op.PersoonId == user);

			if (opleidingPersoon == null)
			{
				return NotFound("Gebruiker is niet ingeschreven voor deze opleiding.");
			}

			// Verwijder de OpleidingPersoon
			opleiding.OpleidingPersonen.Remove(opleidingPersoon);
			opleiding.IngeschrevenPersonen = opleiding.OpleidingPersonen.Count;
			// Sla de wijzigingen op
			_unitOfWork.SaveChanges();

			// Redirect naar de details van de opleiding
			return RedirectToAction(nameof(Details), new { id = opleiding.Id });
		}



		#endregion
	}
}
