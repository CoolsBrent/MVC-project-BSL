using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MVC_Project_BSL.Models;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using MVC_Project_BSL.Data;
using Microsoft.AspNetCore.Authorization;
using MVC_Project_BSL.Data.UnitOfWork;
using MVC_Project_BSL.ViewModels;
using MVC_Project_BSL.ViewModels.MVC_Project_BSL.ViewModels;

namespace MVC_Project_BSL.Controllers
{
    //Dit is de oude code om op terug te kunnen vallen indien nodig

    //public class GroepsreisController : Controller
    //{
    //    private readonly ApplicationDbContext _context;

    //    public GroepsreisController(ApplicationDbContext context)
    //    {
    //        _context = context;
    //    }

    //    // GET: Groepsreizen
    //    public async Task<IActionResult> Index()
    //    {
    //        var groepsreizen = await _context.Groepsreizen
    //            .Include(g => g.Bestemming)
    //            .ToListAsync();

    //        // Geen verdere acties nodig, de view toont de melding als er geen groepsreizen zijn.

    //        return View(groepsreizen);
    //    }


    //    // GET: Groepsreizen/Create
    //    public IActionResult Create()
    //    {
    //        ViewBag.Bestemmingen = new SelectList(_context.Bestemmingen, "Id", "BestemmingsNaam");
    //        return View();
    //    }

    //    // POST: Groepsreizen/Create
    //    [HttpPost]
    //    [ValidateAntiForgeryToken]
    //    public async Task<IActionResult> Create(Groepsreis groepsreis)
    //    {
    //        if (ModelState.IsValid)
    //        {
    //            _context.Add(groepsreis);
    //            await _context.SaveChangesAsync();
    //            return RedirectToAction(nameof(Index));
    //        }
    //        ViewBag.Bestemmingen = new SelectList(_context.Bestemmingen, "Id", "BestemmingsNaam", groepsreis.BestemmingId);
    //        return View(groepsreis);
    //    }

    //    // GET: Groepsreizen/Edit/5
    //    public async Task<IActionResult> Edit(int id)
    //    {
    //        var groepsreis = await _context.Groepsreizen.FindAsync(id);
    //        if (groepsreis == null)
    //        {
    //            return NotFound();
    //        }
    //        ViewBag.Bestemmingen = new SelectList(_context.Bestemmingen, "Id", "BestemmingsNaam", groepsreis.BestemmingId);
    //        return View(groepsreis);
    //    }

    //    // POST: Groepsreizen/Edit/5
    //    [HttpPost]
    //    [ValidateAntiForgeryToken]
    //    public async Task<IActionResult> Edit(int id, Groepsreis groepsreis)
    //    {
    //        if (id != groepsreis.Id)
    //        {
    //            return NotFound();
    //        }

    //        if (ModelState.IsValid)
    //        {
    //            try
    //            {
    //                _context.Update(groepsreis);
    //                await _context.SaveChangesAsync();
    //            }
    //            catch (DbUpdateConcurrencyException)
    //            {
    //                if (!GroepsreisExists(groepsreis.Id))
    //                {
    //                    return NotFound();
    //                }
    //                else
    //                {
    //                    throw;
    //                }
    //            }
    //            return RedirectToAction(nameof(Index));
    //        }
    //        ViewBag.Bestemmingen = new SelectList(_context.Bestemmingen, "Id", "BestemmingsNaam", groepsreis.BestemmingId);
    //        return View(groepsreis);
    //    }

    //    // GET: Groepsreizen/Delete/5
    //    public async Task<IActionResult> Delete(int id)
    //    {
    //        var groepsreis = await _context.Groepsreizen
    //            .Include(g => g.Bestemming)
    //            .FirstOrDefaultAsync(m => m.Id == id);
    //        if (groepsreis == null)
    //        {
    //            return NotFound();
    //        }
    //        return View(groepsreis);
    //    }

    //    // POST: Groepsreizen/Delete/5
    //    [HttpPost, ActionName("Delete")]
    //    [ValidateAntiForgeryToken]
    //    public async Task<IActionResult> DeleteConfirmed(int id)
    //    {
    //        var groepsreis = await _context.Groepsreizen.FindAsync(id);
    //        _context.Groepsreizen.Remove(groepsreis);
    //        await _context.SaveChangesAsync();
    //        return RedirectToAction(nameof(Index));
    //    }

    //    private bool GroepsreisExists(int id)
    //    {
    //        return _context.Groepsreizen.Any(e => e.Id == id);
    //    }
    //}

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
                groepsreis.Kinderen = groepsreis.Kinderen ?? new List<Kind>();
                groepsreis.Monitoren = groepsreis.Monitoren ?? new List<Models.Monitor>();
                groepsreis.Onkosten = groepsreis.Onkosten ?? new List<Onkosten>();
                groepsreis.Activiteiten = groepsreis.Activiteiten ?? new List<Activiteit>();


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

            ViewBag.Activiteiten = new SelectList(await _unitOfWork.ActiviteitRepository.GetAllAsync(), "Id", "Naam", groepsreis.Activiteiten);
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

            ViewBag.Activiteiten = new SelectList(await _unitOfWork.ActiviteitRepository.GetAllAsync(), "Id", "Naam", groepsreis.Activiteiten);
            return View(groepsreis);
        }

        // GET: Groepsreis/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var groepsreis = await _unitOfWork.GroepsreisRepository.GetByIdAsync(id);
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
                               .Include(g => g.Kinderen));

            // Zoek de specifieke groepsreis met het gegeven id
            var groepsreis = groepsreizen.FirstOrDefault(g => g.Id == id);

            var ingeschrevenMonitoren = groepsreis?.Monitoren.Select(m => m.PersoonId).ToList();
            var uniekeMonitoren = monitoren
                    .Where(m => !ingeschrevenMonitoren.Contains(m.PersoonId))
                    .GroupBy(m => m.PersoonId)
                    .Select(g => g.First())
                    .ToList();

            var ingeschrevenDeelnemers = groepsreis?.Kinderen.Select(m => m.Id).ToList();
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
        

    }
}
