using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MVC_Project_BSL.Data.UnitOfWork;
using MVC_Project_BSL.Models;
using MVC_Project_BSL.Services;
using MVC_Project_BSL.ViewModels;
using System.Diagnostics;

namespace MVC_Project_BSL.Controllers
{
	/// <summary>
	/// De GroepsreisController biedt functionaliteiten voor het beheren van groepsreizen, 
	/// inclusief het aanmaken, bewerken, archiveren en activeren van groepsreizen. 
	/// Ook beheert deze controller de deelname van deelnemers en monitoren aan specifieke reizen.
	/// </summary>
	public class GroepsreisController : Controller
	{
		#region Fields and Constructor
		private readonly IUnitOfWork _unitOfWork;
		private readonly MonitorService _monitorService;

		public GroepsreisController(IUnitOfWork unitOfWork, MonitorService monitorService)
		{
			_unitOfWork = unitOfWork;
			_monitorService = monitorService;
		}
		#endregion

		#region Index and Details Actions

		public async Task<IActionResult> Index()
		{
			var actieveGroepsreizen = await _unitOfWork.GroepsreisRepository.GetAllAsync(
				query => query.Include(g => g.Bestemming)
							  .Include(g => g.Deelnemers)
							  .Where(g => !g.IsArchived));
			var gearchiveerdeGroepsreizen = await _unitOfWork.GroepsreisRepository.GetAllAsync(
				query => query.Include(g => g.Bestemming)
							  .Include(g => g.Deelnemers)
							  .Where(g => g.IsArchived));

			var viewModel = new GroepsreisViewModel
			{
				ActieveGroepsreizen = actieveGroepsreizen,
				GearchiveerdeGroepsreizen = gearchiveerdeGroepsreizen
			};

			return View(viewModel);
		}
		[AllowAnonymous]
		public async Task<IActionResult> Detail(int id)
		{
			var monitoren = await _unitOfWork.MonitorRepository.GetAllAsync(
				query => query.Include(m => m.Persoon).Where(m => m.Persoon.IsActief));
			var deelnemers = await _unitOfWork.KindRepository.GetAllAsync(
				query => query.Include(m => m.Persoon));
			var groepsreis = await _unitOfWork.GroepsreisRepository.GetQueryable()
				.Include(g => g.Monitoren).ThenInclude(m => m.Monitor.Persoon)
				.Include(g => g.Bestemming).ThenInclude(b => b.Fotos)
				.Include(g => g.Deelnemers).ThenInclude(d => d.Kind)
				.Include(g => g.Programmas).ThenInclude(p => p.Activiteit)
				.FirstOrDefaultAsync(g => g.Id == id);

			if (groepsreis == null)
			{
				return NotFound();
			}

			// Bereken gemiddelde reviewscore
			var reviews = groepsreis.Deelnemers.Where(d => d.ReviewScore.HasValue).ToList();
			var gemiddeldeScore = reviews.Any() ? reviews.Average(d => d.ReviewScore.Value) : 0;

			var ingeschrevenMonitoren = groepsreis.Monitoren.Select(m => m.Monitor.PersoonId).ToList();
			var uniekeMonitoren = monitoren.Where(m => !ingeschrevenMonitoren.Contains(m.PersoonId)).ToList();
			var ingeschrevenDeelnemers = groepsreis.Deelnemers.Select(m => m.KindId).ToList();
			var uniekeDeelnemers = deelnemers.Where(m => !ingeschrevenDeelnemers.Contains(m.Id)).ToList();

			groepsreis.BeschikbareMonitoren = uniekeMonitoren;
			groepsreis.BeschikbareDeelnemers = uniekeDeelnemers;

			// ViewBag gebruiken voor eenvoudigheid (of voeg een nieuwe property toe aan het ViewModel)
			ViewBag.Reviews = reviews;
			ViewBag.GemiddeldeScore = gemiddeldeScore;

			// Haal de ID van de ingelogde gebruiker
			var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
			
            if (userId != 0)
            {
				var user = await _unitOfWork.CustomUserRepository.GetQueryable()
				.Include(u => u.Kinderen)
				.FirstOrDefaultAsync(u => u.Id == userId);
				if (user == null)
				{
					Debug.WriteLine($"Gebruiker met ID {userId} niet gevonden.");
					return Unauthorized();
				}

				// Check of de gebruiker een beheerder is
				var isAdmin = User.IsInRole("Beheerder");

				// Beschikbare deelnemers filteren
				if (isAdmin)
				{
					// Als admin, toon alle kinderen die niet zijn ingeschreven en binnen de leeftijdscategorie vallen
					var alleKinderen = await _unitOfWork.KindRepository.GetAllAsync(
						query => query.Include(k => k.Persoon));

					groepsreis.BeschikbareDeelnemers = alleKinderen
						.Where(k => !groepsreis.Deelnemers.Any(d => d.KindId == k.Id) &&
									IsLeeftijdToegestaan(k.Geboortedatum, groepsreis.Bestemming.MinLeeftijd, groepsreis.Bestemming.MaxLeeftijd))
						.ToList();

					Debug.WriteLine($"Admin heeft {groepsreis.BeschikbareDeelnemers.Count} kinderen gevonden.");
				}
				else
				{
					// Voor een gewone gebruiker, toon alleen hun eigen kinderen die beschikbaar zijn
					if (user.Kinderen == null || !user.Kinderen.Any())
					{
						Debug.WriteLine($"Gebruiker met ID {userId} heeft geen kinderen gekoppeld.");
					}
					else
					{
						Debug.WriteLine($"Kinderen van gebruiker {userId}:");
						foreach (var kind in user.Kinderen)
						{
							var leeftijd = CalculateLeeftijd(kind.Geboortedatum);
							Debug.WriteLine($"Kind: {kind.Voornaam} {kind.Naam}, Leeftijd: {leeftijd}");
						}
					}

					groepsreis.BeschikbareDeelnemers = user.Kinderen
						.Where(k => !groepsreis.Deelnemers.Any(d => d.KindId == k.Id) &&
									IsLeeftijdToegestaan(k.Geboortedatum, groepsreis.Bestemming.MinLeeftijd, groepsreis.Bestemming.MaxLeeftijd))
						.ToList();

					Debug.WriteLine($"Beschikbare kinderen voor gebruiker {userId}: {groepsreis.BeschikbareDeelnemers.Count}");
				}
			}
			
            else
            {
				// Geen gebruiker ingelogd, logica voor bezoekers
				Debug.WriteLine("Bezoeker heeft geen kinderen gekoppeld.");
                // Pas logica aan voor bezoekers hier
                return View(groepsreis);
			}


			// Beschikbare monitoren filteren
			monitoren = await _unitOfWork.MonitorRepository.GetAllAsync(
				query => query.Include(m => m.Persoon).Where(m => m.Persoon.IsActief));
			var ingeschrevenMonitorenIds = groepsreis.Monitoren.Select(m => m.Monitor.PersoonId).ToList();
			groepsreis.BeschikbareMonitoren = monitoren
				.Where(m => !ingeschrevenMonitorenIds.Contains(m.PersoonId))
				.ToList();

			return View(groepsreis);
		}

		private bool IsLeeftijdToegestaan(DateTime geboortedatum, int minLeeftijd, int maxLeeftijd)
		{
			var leeftijd = CalculateLeeftijd(geboortedatum);
			Debug.WriteLine($"Leeftijd berekend: {leeftijd}, Toegestaan: {minLeeftijd} - {maxLeeftijd}");
			return leeftijd >= minLeeftijd && leeftijd <= maxLeeftijd;
		}

		// Helper-methode om de leeftijd van een persoon te berekenen
		private int CalculateLeeftijd(DateTime geboortedatum)
		{
			var leeftijd = DateTime.Now.Year - geboortedatum.Year;
			if (DateTime.Now < geboortedatum.AddYears(leeftijd)) leeftijd--; // Corrigeer voor niet-gepasseerde verjaardag
			return leeftijd;
		}


		public async Task<IActionResult> ArchivedDetail(int id)
		{
			var groepsreis = await _unitOfWork.GroepsreisRepository.GetQueryable(
				query => query.Include(g => g.Monitoren).ThenInclude(m => m.Monitor.Persoon)
							  .Include(g => g.Bestemming).ThenInclude(b => b.Fotos)
							  .Include(g => g.Deelnemers)).FirstOrDefaultAsync(g => g.Id == id);
			var deelnemers = await _unitOfWork.KindRepository.GetAllAsync(
				query => query.Include(m => m.Persoon));
			var monitoren = await _unitOfWork.MonitorRepository.GetAllAsync(
				query => query.Include(m => m.Persoon));


			return groepsreis == null ? NotFound() : View(groepsreis);
		}

		#endregion

		#region Create, Edit and Delete Actions

		public IActionResult Create()
		{
			LoadDropdownData();
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(Groepsreis groepsreis)
		{
			LogModelStateErrors();

			if (ModelState.IsValid)
			{
				InitializeCollections(groepsreis);
				await _unitOfWork.GroepsreisRepository.AddAsync(groepsreis);
				_unitOfWork.SaveChanges();

				return RedirectToAction(nameof(Index));
			}

			LoadDropdownData(groepsreis);
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
				groepsreis.Programmas);

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
					groepsreis.Programmas);

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
				bestaandeGroepsreis.MaxAantalDeelnemers = groepsreis.MaxAantalDeelnemers;

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


		#endregion

		#region Archive and Activate Actions

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Archive(int id)
		{
			await ToggleGroepsreisArchiveStatus(id, true);
			return RedirectToAction(nameof(Index));
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Activate(int id)
		{
			await ToggleGroepsreisArchiveStatus(id, false);
			return RedirectToAction(nameof(Index));
		}

		#endregion

		#region Monitor and Participant Management

		[HttpGet]
		public async Task<IActionResult> BeschikbareKinderen(int groepsreisId)
		{
			// Haal het ID van de ingelogde gebruiker op
			var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
			if (userIdClaim == null) return Unauthorized();

			var gebruikerId = int.Parse(userIdClaim.Value);

			// Haal de groepsreis op
			var groepsreis = await _unitOfWork.GroepsreisRepository.GetByIdAsync(groepsreisId);
			if (groepsreis == null) return NotFound();

			// Haal de kinderen van de ingelogde gebruiker op
			var kinderen = await _unitOfWork.KindRepository.GetQueryable()
				.Include(k => k.Persoon)
				.Where(k => k.PersoonId == gebruikerId)
				.ToListAsync();

			// Filter kinderen die voldoen aan leeftijdscriteria en nog niet zijn ingeschreven
			var beschikbareKinderen = kinderen
				.Where(k =>
					!groepsreis.Deelnemers.Any(d => d.KindId == k.Id) && // Nog niet ingeschreven
					k.Geboortedatum <= DateTime.Now.AddYears(-groepsreis.Bestemming.MinLeeftijd) && // Oud genoeg
					k.Geboortedatum >= DateTime.Now.AddYears(-groepsreis.Bestemming.MaxLeeftijd)) // Niet te oud
				.ToList();

			return PartialView("_BeschikbareKinderen", beschikbareKinderen);
		}


		[HttpPost]
		public async Task<IActionResult> VoegDeelnemerToe(int groepsreisId, int kindId)
		{
			// Haal groepsreis op inclusief deelnemers en wachtlijst
			var groepsreis = await _unitOfWork.GroepsreisRepository.GetQueryable()
				.Include(g => g.Deelnemers)
				.Include(g => g.Wachtlijst)
				.FirstOrDefaultAsync(g => g.Id == groepsreisId);

			if (groepsreis == null)
				return RedirectToAction("Detail", new { id = groepsreisId });

			// Controleer of het kind al bestaat
			var kind = await _unitOfWork.KindRepository.GetQueryable()
				.FirstOrDefaultAsync(k => k.Id == kindId);

			if (kind == null)
				return RedirectToAction("Detail", new { id = groepsreisId });

			// Controleer of het kind al in deelnemers of wachtlijst zit
			if (groepsreis.Deelnemers.Any(d => d.KindId == kindId) || groepsreis.Wachtlijst.Any(w => w.KindId == kindId))
				return RedirectToAction("Detail", new { id = groepsreisId });

			// Controleer of groepsreis vol is
			if (groepsreis.Deelnemers.Count >= groepsreis.MaxAantalDeelnemers)
			{
				// Voeg toe aan de wachtlijst
				groepsreis.Wachtlijst.Add(new Deelnemer { KindId = kindId, GroepsreisDetailId = groepsreisId });

				// Controleer of wachtlijst groot genoeg is om nieuwe groepsreis te maken
				if (groepsreis.Wachtlijst.Count >= Math.Floor(groepsreis.MaxAantalDeelnemers * 0.8))
				{
					await MaakNieuweGroepsreisVanWachtlijst(groepsreis);
				}

				_unitOfWork.SaveChanges();
				return RedirectToAction("Detail", new { id = groepsreisId });
			}

			// Voeg toe aan de deelnemerslijst
			groepsreis.Deelnemers.Add(new Deelnemer { KindId = kindId, GroepsreisDetailId = groepsreisId });

			_unitOfWork.SaveChanges();
			return RedirectToAction("Detail", new { id = groepsreisId });
		}





		private async Task MaakNieuweGroepsreisVanWachtlijst(Groepsreis origineleGroepsreis)
		{
			if (origineleGroepsreis == null || origineleGroepsreis.Wachtlijst == null)
			{
				Debug.WriteLine("De originele groepsreis of de wachtlijst is null.");
				return;
			}

			if (!origineleGroepsreis.Wachtlijst.Any())
			{
				Debug.WriteLine("Geen deelnemers op de wachtlijst.");
				return;
			}

			var nieuweGroepsreis = new Groepsreis
			{
				Begindatum = origineleGroepsreis.Begindatum,
				Einddatum = origineleGroepsreis.Einddatum,
				Prijs = origineleGroepsreis.Prijs,
				BestemmingId = origineleGroepsreis.BestemmingId,
				MaxAantalDeelnemers = origineleGroepsreis.MaxAantalDeelnemers,
				Deelnemers = new List<Deelnemer>()
			};

			var deelnemersVoorNieuweGroepsreis = origineleGroepsreis.Wachtlijst
				.Take(nieuweGroepsreis.MaxAantalDeelnemers)
				.ToList();

			foreach (var deelnemer in deelnemersVoorNieuweGroepsreis)
			{
				if (deelnemer == null) continue;

				nieuweGroepsreis.Deelnemers.Add(deelnemer);
				origineleGroepsreis.Wachtlijst.Remove(deelnemer); // Zorgt dat het correct uit de originele lijst verdwijnt
			}

			await _unitOfWork.GroepsreisRepository.AddAsync(nieuweGroepsreis);
			_unitOfWork.SaveChanges();

			Debug.WriteLine($"Nieuwe groepsreis aangemaakt met ID: {nieuweGroepsreis.Id}, Aantal deelnemers: {nieuweGroepsreis.Deelnemers.Count}.");
		}


		[HttpPost]
		public async Task<IActionResult> MaakNieuweGroep(int groepsreisId)
		{
			var groepsreis = await _unitOfWork.GroepsreisRepository.GetByIdAsync(groepsreisId);
			if (groepsreis == null || !groepsreis.Wachtlijst.Any())
			{
				return NotFound();
			}

			var nieuweGroepsreis = new Groepsreis
			{
				Begindatum = groepsreis.Begindatum,
				Einddatum = groepsreis.Einddatum,
				Prijs = groepsreis.Prijs,
				BestemmingId = groepsreis.BestemmingId,
				MaxAantalDeelnemers = groepsreis.MaxAantalDeelnemers
			};

			// Verplaats wachtlijstdeelnemers naar de nieuwe groep
			while (groepsreis.Wachtlijst.Any() && nieuweGroepsreis.Deelnemers.Count < nieuweGroepsreis.MaxAantalDeelnemers)
			{
				var deelnemer = groepsreis.Wachtlijst.First();
				groepsreis.Wachtlijst.Remove(deelnemer);
				nieuweGroepsreis.Deelnemers.Add(deelnemer);
			}

			await _unitOfWork.GroepsreisRepository.AddAsync(nieuweGroepsreis);
			_unitOfWork.SaveChanges();

			return RedirectToAction("Index");
		}

		[HttpPost]
		[HttpPost]
		public async Task<IActionResult> VerplaatsInWachtlijst(int groepsreisId, int kindId, int nieuweIndex)
		{
			// Groepsreis ophalen inclusief wachtlijst
			var groepsreis = await _unitOfWork.GroepsreisRepository.GetQueryable()
				.Include(g => g.Wachtlijst)
				.FirstOrDefaultAsync(g => g.Id == groepsreisId);

			if (groepsreis == null)
				return NotFound("Groepsreis niet gevonden.");

			// Zet de wachtlijst om naar een lijst
			var wachtlijst = groepsreis.Wachtlijst.ToList();

			// Zoek de deelnemer in de wachtlijst
			var deelnemer = wachtlijst.FirstOrDefault(w => w.KindId == kindId);
			if (deelnemer == null)
				return NotFound("Deelnemer niet gevonden in de wachtlijst.");

			// Controleer of de nieuwe index geldig is
			if (nieuweIndex < 0 || nieuweIndex >= wachtlijst.Count)
				return BadRequest("Ongeldige nieuwe index.");

			// Verwijder de deelnemer tijdelijk uit de lijst
			wachtlijst.Remove(deelnemer);

			// Voeg de deelnemer toe op de nieuwe positie
			wachtlijst.Insert(nieuweIndex, deelnemer);

			// Werk de wachtlijst in de groepsreis bij
			groepsreis.Wachtlijst = wachtlijst;

			// Wijzigingen opslaan
			_unitOfWork.SaveChanges();

			return RedirectToAction("Detail", new { id = groepsreisId });
		}



		[HttpPost]
		public async Task<IActionResult> VerwijderUitWachtlijst(int groepsreisId, int kindId)
		{
			// Haal de groepsreis op inclusief de wachtlijst
			var groepsreis = await _unitOfWork.GroepsreisRepository.GetQueryable()
				.Include(g => g.Wachtlijst)
				.Include(g => g.Deelnemers) // Inclusief deelnemerslijst
				.FirstOrDefaultAsync(g => g.Id == groepsreisId);

			if (groepsreis == null)
				return NotFound("Groepsreis niet gevonden.");

			// Zoek het kind in de wachtlijst
			var wachtlijstDeelnemer = groepsreis.Wachtlijst.FirstOrDefault(w => w.KindId == kindId);
			if (wachtlijstDeelnemer != null)
			{
				// Verwijder het kind uit de wachtlijst
				groepsreis.Wachtlijst.Remove(wachtlijstDeelnemer);
			}

			// Controleer of het kind in de deelnemerslijst zit (zou niet moeten gebeuren)
			var deelnemer = groepsreis.Deelnemers.FirstOrDefault(d => d.KindId == kindId);
			if (deelnemer != null)
			{
				groepsreis.Deelnemers.Remove(deelnemer);
			}

			_unitOfWork.SaveChanges();
			return RedirectToAction("Detail", new { id = groepsreisId });
		}



		[HttpPost]
		public async Task<IActionResult> DeleteDeelnemer(int groepsreisId, int kindId)
		{
			Debug.WriteLine($"Verzoek ontvangen om kind met ID {kindId} te verwijderen uit groepsreis met ID {groepsreisId}.");

			// Groepsreis ophalen inclusief deelnemers en wachtlijst
			var groepsreis = await _unitOfWork.GroepsreisRepository.GetQueryable()
				.Include(g => g.Deelnemers)
				.Include(g => g.Wachtlijst)
				.FirstOrDefaultAsync(g => g.Id == groepsreisId);

			if (groepsreis == null)
			{
				Debug.WriteLine($"Groepsreis met ID {groepsreisId} niet gevonden.");
				return NotFound("Groepsreis niet gevonden.");
			}

			// Zoek de deelnemer met het gegeven kindId
			var deelnemer = groepsreis.Deelnemers.FirstOrDefault(d => d.KindId == kindId);
			if (deelnemer == null)
			{
				Debug.WriteLine($"Kind met ID {kindId} is geen deelnemer aan groepsreis met ID {groepsreisId}.");
				return NotFound("Kind is geen deelnemer van deze groepsreis.");
			}

			// Verwijder de deelnemer
			groepsreis.Deelnemers.Remove(deelnemer);
			Debug.WriteLine($"Kind met ID {kindId} succesvol verwijderd uit groepsreis met ID {groepsreisId}.");

			// Controleer of er deelnemers in de wachtlijst staan
			if (groepsreis.Wachtlijst.Any())
			{
				// Haal de eerste persoon uit de wachtlijst (index 0)
				var eerstvolgende = groepsreis.Wachtlijst.First();

				// Verplaats de persoon van de wachtlijst naar de deelnemerslijst
				groepsreis.Deelnemers.Add(eerstvolgende);
				groepsreis.Wachtlijst.Remove(eerstvolgende);

				Debug.WriteLine($"Kind met ID {eerstvolgende.KindId} toegevoegd aan de groepsreis vanuit de wachtlijst.");
			}

			// Wijzigingen opslaan
			_unitOfWork.SaveChanges();
			Debug.WriteLine("Wijzigingen succesvol opgeslagen.");

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

			var groepsreisMonitor = new GroepsreisMonitor
			{
				GroepsreisId = groepsreis.Id,
				MonitorId = monitor.Id
			};

			// Voeg de nieuwe GroepsreisMonitor toe aan de groepsreis
			groepsreis.Monitoren.Add(groepsreisMonitor);

			// Sla de wijzigingen op
			_unitOfWork.SaveChanges();


			return RedirectToAction("Detail", new { id = groepsreisId });
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

		#endregion

		#region Helper Methods

		private void LoadDropdownData(Groepsreis groepsreis = null)
		{
			ViewBag.Bestemmingen = new SelectList(_unitOfWork.BestemmingRepository.GetAllAsync().Result, "Id", "BestemmingsNaam", groepsreis?.BestemmingId);
			ViewBag.Activiteiten = new SelectList(_unitOfWork.ActiviteitRepository.GetAllAsync().Result, "Id", "Naam");
		}

		private async Task<string> SaveFotoAsync(IFormFile fotoFile)
		{
			var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/onkosten");
			Directory.CreateDirectory(uploadsFolder);

			var uniekeBestandsnaam = Guid.NewGuid().ToString() + Path.GetExtension(fotoFile.FileName);
			var bestandspad = Path.Combine(uploadsFolder, uniekeBestandsnaam);

			using (var fileStream = new FileStream(bestandspad, FileMode.Create))
			{
				await fotoFile.CopyToAsync(fileStream);
			}

			return "/uploads/onkosten/" + uniekeBestandsnaam;
		}

		private void InitializeCollections(Groepsreis groepsreis)
		{
			groepsreis.Deelnemers ??= new List<Deelnemer>();
			groepsreis.Monitoren ??= new List<GroepsreisMonitor>();
			groepsreis.Onkosten ??= new List<Onkosten>();
			groepsreis.Programmas ??= new List<Programma>();
		}

		private async Task ToggleGroepsreisArchiveStatus(int id, bool isArchived)
		{
			var groepsreis = await _unitOfWork.GroepsreisRepository.GetByIdAsync(id);
			if (groepsreis == null) return;

			groepsreis.IsArchived = isArchived;
			_unitOfWork.GroepsreisRepository.Update(groepsreis);
			_unitOfWork.SaveChanges();
		}

		private async Task UpdateGroepsreis(int id, Groepsreis groepsreis)
		{
			var bestaandeGroepsreis = await _unitOfWork.GroepsreisRepository.GetQueryable(
				query => query.Include(g => g.Onkosten)).FirstOrDefaultAsync(g => g.Id == id);

			if (bestaandeGroepsreis == null) return;

			bestaandeGroepsreis.Begindatum = groepsreis.Begindatum;
			bestaandeGroepsreis.Einddatum = groepsreis.Einddatum;
			bestaandeGroepsreis.Prijs = groepsreis.Prijs;
			bestaandeGroepsreis.BestemmingId = groepsreis.BestemmingId;

			_unitOfWork.GroepsreisRepository.Update(bestaandeGroepsreis);
			_unitOfWork.SaveChanges();
		}

		private async Task ManageDeelnemerInGroepsreis(int groepsreisId, int kindId, bool toevoegen)
		{
			var groepsreis = await _unitOfWork.GroepsreisRepository.GetByIdAsync(groepsreisId);
			var kind = await _unitOfWork.KindRepository.GetByIdAsync(kindId);

			if (groepsreis == null || kind == null) return;

			if (toevoegen)
			{
				groepsreis.Deelnemers.Add(new Deelnemer { KindId = kind.Id, GroepsreisDetailId = groepsreis.Id });
			}
			else
			{
				var deelnemer = groepsreis.Deelnemers.FirstOrDefault(d => d.KindId == kindId);
				if (deelnemer != null) groepsreis.Deelnemers.Remove(deelnemer);
			}

			_unitOfWork.SaveChanges();
		}

		private async Task ManageMonitorInGroepsreis(int groepsreisId, int monitorId, bool toevoegen)
		{
			var groepsreis = await _unitOfWork.GroepsreisRepository.GetByIdAsync(groepsreisId);
			var monitor = await _unitOfWork.MonitorRepository.GetByIdAsync(monitorId);

			if (groepsreis == null || monitor == null) return;

			if (toevoegen)
			{
				groepsreis.Monitoren.Add(new GroepsreisMonitor { GroepsreisId = groepsreis.Id, MonitorId = monitor.Id });
			}
			else
			{
				var groepsreisMonitor = groepsreis.Monitoren.FirstOrDefault(gm => gm.MonitorId == monitorId);
				if (groepsreisMonitor != null) groepsreis.Monitoren.Remove(groepsreisMonitor);
			}

			_unitOfWork.SaveChanges();
		}

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

		public async Task<JsonResult> GetOnkosten(string term)
		{
			// Haal alle onkosten op
			var onkosten = await _unitOfWork.OnkostenRepository.GetAllAsync();

			// Als er een zoekterm is, filter dan op de term
			if (!string.IsNullOrWhiteSpace(term))
			{
				onkosten = onkosten
					.Where(o => o.Titel.Contains(term, StringComparison.OrdinalIgnoreCase)) // Filteren op de term
					.Take(10) // Maximaal 10 resultaten
					.ToList();
			}
			else
			{
				// Als er geen zoekterm is, geef dan de populairste onkosten terug (je kunt dit aanpassen zoals je wilt)
				onkosten = onkosten
					.Take(10)
					.ToList();
			}

			// Haal de titels van de onkosten
			var onkostenTitels = onkosten
				.Select(o => o.Titel)
				.ToList();

			// Retourneer de resultaten als JSON
			return Json(onkostenTitels);
		}
		public IActionResult Onkosten(int id)
		{
			// Haal de onkosten op voor de groepsreis met het opgegeven id
			var groepsreizen = _unitOfWork.GroepsreisRepository.GetAllAsync();
			var groepsreis = groepsreizen.Result.FirstOrDefault(r => r.Id == id);


			if (groepsreis == null)
			{
				return NotFound();
			}
			return View(groepsreis); // Stuur het volledige groepsreis object naar de view

		}


		[HttpGet]
		public async Task<JsonResult> GetBestemmingen(string term)
		{
			// Log de zoekterm
			Debug.WriteLine($"GetBestemmingen aangeroepen met term: {term}");

			// Simuleer een lijst van bestemmingen als voorbeeld
			var bestemmingen = await _unitOfWork.BestemmingRepository.GetAllAsync();

			if (!string.IsNullOrWhiteSpace(term))
			{
				// Filter bestemmingen
				bestemmingen = bestemmingen
					.Where(b => b.BestemmingsNaam != null && b.BestemmingsNaam.Contains(term, StringComparison.OrdinalIgnoreCase))
					.Take(10)
					.ToList();
			}
			else
			{
				// Geef maximaal 10 resultaten als er geen zoekterm is
				bestemmingen = bestemmingen.Take(10).ToList();
			}

			// Controleer of bestemmingen leeg zijn
			if (!bestemmingen.Any())
			{
				Debug.WriteLine("Geen resultaten gevonden.");
			}
			else
			{
				Debug.WriteLine($"Resultaten gevonden: {string.Join(", ", bestemmingen.Select(b => b.BestemmingsNaam))}");
			}

			// Stuur de resultaten terug
			return Json(bestemmingen.Select(b => b.BestemmingsNaam).ToList());
		}

	}
	#endregion
}
