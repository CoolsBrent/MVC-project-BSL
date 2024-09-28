using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MVC_Project_BSL.Models;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using MVC_Project_BSL.Data;
using Microsoft.AspNetCore.Authorization;

namespace MVC_Project_BSL.Controllers
{
    public class GroepsreisController : Controller
    {
        private readonly ApplicationDbContext _context;

        public GroepsreisController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Groepsreizen
        public async Task<IActionResult> Index()
        {
            var groepsreizen = await _context.Groepsreizen
                .Include(g => g.Bestemming)
                .ToListAsync();

            // Geen verdere acties nodig, de view toont de melding als er geen groepsreizen zijn.

            return View(groepsreizen);
        }


        // GET: Groepsreizen/Create
        public IActionResult Create()
        {
            ViewBag.Bestemmingen = new SelectList(_context.Bestemmingen, "Id", "BestemmingsNaam");
            return View();
        }

        // POST: Groepsreizen/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Groepsreis groepsreis)
        {
            if (ModelState.IsValid)
            {
                _context.Add(groepsreis);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Bestemmingen = new SelectList(_context.Bestemmingen, "Id", "BestemmingsNaam", groepsreis.BestemmingId);
            return View(groepsreis);
        }

        // GET: Groepsreizen/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var groepsreis = await _context.Groepsreizen.FindAsync(id);
            if (groepsreis == null)
            {
                return NotFound();
            }
            ViewBag.Bestemmingen = new SelectList(_context.Bestemmingen, "Id", "BestemmingsNaam", groepsreis.BestemmingId);
            return View(groepsreis);
        }

        // POST: Groepsreizen/Edit/5
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
                    _context.Update(groepsreis);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GroepsreisExists(groepsreis.Id))
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
            ViewBag.Bestemmingen = new SelectList(_context.Bestemmingen, "Id", "BestemmingsNaam", groepsreis.BestemmingId);
            return View(groepsreis);
        }

        // GET: Groepsreizen/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var groepsreis = await _context.Groepsreizen
                .Include(g => g.Bestemming)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (groepsreis == null)
            {
                return NotFound();
            }
            return View(groepsreis);
        }

        // POST: Groepsreizen/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var groepsreis = await _context.Groepsreizen.FindAsync(id);
            _context.Groepsreizen.Remove(groepsreis);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool GroepsreisExists(int id)
        {
            return _context.Groepsreizen.Any(e => e.Id == id);
        }
    }
}
