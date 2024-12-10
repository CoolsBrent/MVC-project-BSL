using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MVC_Project_BSL.Data.UnitOfWork;
using MVC_Project_BSL.Models;
using System.IO;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

public class OnkostenController : Controller
{
    private readonly IUnitOfWork _unitOfWork;

    public OnkostenController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    // GET: Onkosten/Index
    public async Task<IActionResult> Index(int groepsreisId)
    {
        var groepsreis = await _unitOfWork.GroepsreisRepository.GetQueryable(
            query => query
                .Include(g => g.Onkosten)
                .Include(g => g.Bestemming))
            .FirstOrDefaultAsync(g => g.Id == groepsreisId);

        if (groepsreis == null)
        {
            return NotFound("Groepsreis niet gevonden.");
        }

        ViewBag.GroepsreisId = groepsreis.Id;
        ViewBag.GroepsreisNaam = groepsreis.Bestemming.BestemmingsNaam; // Optioneel: voeg groepsreisnaam toe voor context
        return View(groepsreis.Onkosten); // Stuur de lijst met onkosten naar de view
    }

    // CREATE
    // GET: Onkosten/Create
    public IActionResult Create(int groepsreisId)
    {
        var groepsreis = _unitOfWork.GroepsreisRepository.GetByIdAsync(groepsreisId);
        if (groepsreis == null)
        {
            return NotFound("Groepsreis niet gevonden.");
        }

        // Stel een nieuw Onkosten object in voor de view
        var onkosten = new Onkosten { GroepsreisId = groepsreisId };

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
