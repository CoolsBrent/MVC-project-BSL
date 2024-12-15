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
            return Unauthorized();
        }

        var gebruiker = await _userManager.FindByIdAsync(userId);
        if (gebruiker == null)
        {
            return NotFound("Gebruiker niet gevonden.");
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
        var hoofdmonitorOnkosten = groepsreis.Onkosten
            .Where(o => o.TypeOnkost == "Hoofdmonitor")
            .ToList();

        var verantwoordelijkeOnkosten = groepsreis.Onkosten
            .Where(o => o.TypeOnkost == "Verantwoordelijke")
            .ToList();

        // Bereken budget en resterend budget voor hoofdmonitor
        var aantalDeelnemers = groepsreis.Deelnemers?.Count ?? 0;
        var totaalPrijs = aantalDeelnemers * (groepsreis?.Prijs ?? 0);
        var budget = totaalPrijs * 0.3; // Budget is 30% van totale opbrengst
        var totaleHoofdmonitorOnkosten = hoofdmonitorOnkosten.Sum(o => o.Bedrag);
        var resterendBudget = budget - totaleHoofdmonitorOnkosten;

        ViewBag.ResterendBudget = resterendBudget;
        ViewBag.GroepsreisId = groepsreis.Id;
        ViewBag.GroepsreisNaam = groepsreis.Bestemming.BestemmingsNaam;

        // Maak een ViewModel
        var model = new Onkosten
        {
            VerantwoordelijkeOnkosten = verantwoordelijkeOnkosten,
            HoofdmonitorOnkosten = hoofdmonitorOnkosten,
            AlleOnkosten = groepsreis.Onkosten.ToList(),
            Groepsreis = groepsreis
        };

        return View(model);
    }

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

        // Bereken budget en resterend budget voor hoofdmonitor
        var aantalDeelnemers = groepsreis.Deelnemers?.Count ?? 0;
        var totaalPrijs = aantalDeelnemers * (groepsreis?.Prijs ?? 0);
        var budget = totaalPrijs * 0.3;
        var hoofdmonitorOnkosten = groepsreis.Onkosten.Where(o => o.TypeOnkost == "Hoofdmonitor").Sum(o => o.Bedrag);
        var resterendBudget = budget - hoofdmonitorOnkosten;

        ViewBag.ResterendBudget = resterendBudget;
        ViewBag.GroepsreisNaam = groepsreis.Bestemming.BestemmingsNaam;

        var onkosten = new Onkosten
        {
            GroepsreisId = groepsreisId
        };

        return View(onkosten);
    }

    // POST: Onkosten/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Onkosten onkosten)
    {
        // Haal de groepsreis op
        var groepsreis = await _unitOfWork.GroepsreisRepository.GetQueryable()
            .Include(g => g.Onkosten)
            .Include(g => g.Deelnemers)
            .Include(g => g.Bestemming)
            .FirstOrDefaultAsync(g => g.Id == onkosten.GroepsreisId);

        if (groepsreis == null)
        {
            return NotFound("Groepsreis niet gevonden.");
        }

        // Bereken budget en resterend budget voor hoofdmonitor
        var aantalDeelnemers = groepsreis.Deelnemers?.Count ?? 0;
        var totaalPrijs = aantalDeelnemers * (groepsreis?.Prijs ?? 0);
        var budget = totaalPrijs * 0.3;
        var hoofdmonitorOnkosten = groepsreis.Onkosten.Where(o => o.TypeOnkost == "Hoofdmonitor").Sum(o => o.Bedrag);
        var resterendBudget = budget - hoofdmonitorOnkosten;

        // Controleer overschrijding budget
        if (onkosten.TypeOnkost == "Hoofdmonitor" && (resterendBudget - onkosten.Bedrag) < 0)
        {
            ModelState.AddModelError("", "Het budget voor de hoofdmonitor is overschreden.");
        }

        if (ModelState.IsValid)
        {
            // Verwerk foto (optioneel)
            if (onkosten.FotoFile != null)
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", onkosten.FotoFile.FileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await onkosten.FotoFile.CopyToAsync(stream);
                }
                onkosten.Foto = "/uploads/" + onkosten.FotoFile.FileName;
            }

            groepsreis.Onkosten.Add(onkosten);
            _unitOfWork.GroepsreisRepository.Update(groepsreis);
            _unitOfWork.SaveChanges();
            if (User.IsInRole("Verantwoordelijke") || User.IsInRole("Beheerder"))
            {
                return RedirectToAction(nameof(Index), new { groepsreisId = onkosten.GroepsreisId });

            }
            else if (User.IsInRole("Hoofdmonitor"))
            {
                return RedirectToAction("Detail", "Groepsreis", new { id = onkosten.GroepsreisId });
            }
        }

        ViewBag.ResterendBudget = resterendBudget;
        ViewBag.GroepsreisNaam = groepsreis.Bestemming.BestemmingsNaam;

        return View(onkosten);
    }

    // EDIT
    // GET: Onkosten/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var onkosten = await _unitOfWork.OnkostenRepository.GetQueryable()
            .AsNoTracking()
            .Include(o => o.Groepsreis) // Include de Groepsreis
            .ThenInclude(g => g.Bestemming) // Include de Bestemming via Groepsreis
            .FirstOrDefaultAsync(o => o.Id == id);

        if (onkosten == null)
        {
            return NotFound();
        }

        // Sla de originele GroepsreisId en GroepsreisNaam op voor de weergave
        ViewBag.GroepsreisId = onkosten.GroepsreisId;
        ViewBag.GroepsreisNaam = onkosten.Groepsreis?.Bestemming?.BestemmingsNaam;

        // Bewaar het TypeOnkost van de onkost in ViewBag voor de POST actie
        ViewBag.OrigineleTypeOnkost = onkosten.TypeOnkost;

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
                // Haal de originele onkost op uit de database
                var originalOnkost = await _unitOfWork.OnkostenRepository.GetQueryable()
                 .AsNoTracking()  // Dit zorgt ervoor dat de originele entiteit niet wordt gevolgd
                 .FirstOrDefaultAsync(o => o.Id == onkosten.Id);

                if (originalOnkost == null)
                {
                    return NotFound();
                }

                // Zorg ervoor dat TypeOnkost behouden blijft, zelfs als de verantwoordelijke bewerkt
                onkosten.TypeOnkost = originalOnkost.TypeOnkost;

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

                // Update de onkosten in de database
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
