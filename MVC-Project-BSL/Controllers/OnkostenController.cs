using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MVC_Project_BSL.Data.UnitOfWork;
using MVC_Project_BSL.Models;
using System.IO;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using MVC_Project_BSL.ViewModels;

public class OnkostenController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<CustomUser> _userManager;


    public OnkostenController(IUnitOfWork unitOfWork, UserManager<CustomUser> userManager)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
    }

    // GET: Onkosten/Index
    public async Task<IActionResult> Index(int groepsreisId)
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

        // Haal de groepsreis op
        var groepsreis = await _unitOfWork.GroepsreisRepository.GetQueryable(
            query => query
                .Include(g => g.Onkosten)
                .Include(g => g.Bestemming)
                .Include(g => g.Deelnemers))
            .FirstOrDefaultAsync(g => g.Id == groepsreisId);

        if (groepsreis == null)
        {
            return NotFound("Groepsreis niet gevonden.");
        }

        // Splits de onkosten op in twee groepen
        var VerantwoordelijkeOnkosten = groepsreis.Onkosten
            .Where(o => o.TypeOnkost == "Verantwoordelijke")
            .ToList();

        var HoofdmonitorOnkosten = groepsreis.Onkosten
            .Where(o => o.TypeOnkost == "Hoofdmonitor")
            .ToList();
        var onkost = groepsreis.Onkosten.ToList();

     
        // Zet de groepsreisgegevens in de ViewBag voor context in de view
        ViewBag.GroepsreisId = groepsreis.Id;
        ViewBag.GroepsreisNaam = groepsreis.Bestemming.BestemmingsNaam;

        // Maak een ViewModel om de gesplitste gegevens door te geven aan de view
        var model = new Onkosten
        {
            VerantwoordelijkeOnkosten = VerantwoordelijkeOnkosten,
            HoofdmonitorOnkosten = HoofdmonitorOnkosten,
            AlleOnkosten = onkost,
            Groepsreis = groepsreis,
        };

        return View(model); // Stuur het ViewModel naar de view
    }


    // CREATE
    // GET: Onkosten/Create
    public async Task<IActionResult> Create(int groepsreisId)
    {
        var groepsreis = await _unitOfWork.GroepsreisRepository.GetQueryable(
            query => query
                .Include(g => g.Onkosten)
                .Include(g => g.Bestemming)
                .Include(g => g.Deelnemers))
            .FirstOrDefaultAsync(g => g.Id == groepsreisId);

        if (groepsreis == null)
        {
            return NotFound("Groepsreis niet gevonden.");
        }

        // Bereken het totaal bedrag (prijs per deelnemer * aantal deelnemers)
        var totaalPrijs = groepsreis.Deelnemers?.Count() * groepsreis.Prijs;

        // Budget voor hoofdmonitor is 30% van de totale opbrengst
        var budget = groepsreis.Deelnemers?.Count() * (groepsreis.Prijs * 0.3);

        // Haal de onkosten op voor de specifieke groepsreis
        var onkost = await _unitOfWork.OnkostenRepository
            .GetAllAsync(query => query.Where(o => o.GroepsreisId == groepsreisId) // Filter op GroepsreisId
            .Include(o => o.Groepsreis));

        // Gebruik expliciete selectie en zet het om naar een lijst
        var onkostenList = onkost.Select(o => o.Bedrag).ToList();

        // Bereken de som van de bedragen van de onkosten
        var totaleOnkosten = onkostenList.Sum();

        // Bereken het resterende budget
        var resterendBudget = budget - totaleOnkosten;

        // Stel een nieuw Onkosten object in voor de view
        var onkosten = new Onkosten
        {
            GroepsreisId = groepsreisId
        };

        // Zet de resterende budgetwaarde in de ViewBag om deze naar de view te sturen
        ViewBag.ResterendBudget = resterendBudget;
        ViewBag.GroepsreisNaam = groepsreis.Bestemming.BestemmingsNaam;

        // Stuur het Onkosten object naar de view
        return View(onkosten);
    }


    // POST: Onkosten/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Onkosten onkosten)
    {
        if (ModelState.IsValid)
        {
            // Foto upload logica
            if (onkosten.FotoFile != null)
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", onkosten.FotoFile.FileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await onkosten.FotoFile.CopyToAsync(stream);
                }
                onkosten.Foto = "/uploads/" + onkosten.FotoFile.FileName;
            }

            // Voeg onkosten toe aan de juiste groepsreis
            var groepsreis = await _unitOfWork.GroepsreisRepository.GetQueryable()
                .FirstOrDefaultAsync(g => g.Id == onkosten.GroepsreisId);

            if (groepsreis == null)
            {
                return NotFound("Groepsreis niet gevonden.");
            }

            groepsreis.Onkosten.Add(onkosten);
            _unitOfWork.GroepsreisRepository.Update(groepsreis);
            _unitOfWork.SaveChanges();
            return RedirectToAction(nameof(Index), new { groepsreisId = onkosten.GroepsreisId });
        }
        return View(onkosten);
    }

    // EDIT
    // GET: Onkosten/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var onkosten = await _unitOfWork.OnkostenRepository.GetQueryable()
            .FirstOrDefaultAsync(o => o.Id == id);

        if (onkosten == null)
        {
            return NotFound();
        }

        ViewBag.GroepsreisId = onkosten.GroepsreisId;
        return View(onkosten);
    }

    // POST: Onkosten/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Onkosten onkosten)
    {
        if (id != onkosten.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                // Foto upload logica (optioneel)
                if (onkosten.FotoFile != null)
                {
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", onkosten.FotoFile.FileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await onkosten.FotoFile.CopyToAsync(stream);
                    }
                    onkosten.Foto = "/uploads/" + onkosten.FotoFile.FileName;
                }

                _unitOfWork.OnkostenRepository.Update(onkosten);
                _unitOfWork.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OnkostenExists(onkosten.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index), new { groepsreisId = onkosten.GroepsreisId });
        }
        return View(onkosten);
    }
    // GET: Onkosten/Details/5
    public async Task<IActionResult> Detail(int id)
    {
        // Haal de onkosten op
        var onkost = await _unitOfWork.OnkostenRepository.GetByIdAsync(id);

        if (onkost == null)
        {
            return NotFound();
        }

        return View(onkost);
    }


    // DELETE
    // GET: Onkosten/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        var onkosten = await _unitOfWork.OnkostenRepository.GetQueryable()
            .FirstOrDefaultAsync(o => o.Id == id);

        if (onkosten == null)
        {
            return NotFound();
        }

        return View(onkosten);
    }

    // POST: Onkosten/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var onkosten = await _unitOfWork.OnkostenRepository.GetQueryable()
            .FirstOrDefaultAsync(o => o.Id == id);

        if (onkosten != null)
        {
            _unitOfWork.OnkostenRepository.Delete(onkosten);
            _unitOfWork.SaveChanges();
        }
        return RedirectToAction(nameof(Index), new { groepsreisId = onkosten.GroepsreisId });
    }

    // Helper method to check if Onkosten exists
    private bool OnkostenExists(int id)
    {
        return _unitOfWork.OnkostenRepository.GetQueryable().Any(e => e.Id == id);
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
}
