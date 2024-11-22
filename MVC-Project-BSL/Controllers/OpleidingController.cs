using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MVC_Project_BSL.Data.UnitOfWork;
using MVC_Project_BSL.Models;
using System.Diagnostics;

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
            return View(opleidingen);
        }

        // GET: Opleiding/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var opleiding = await _unitOfWork.OpleidingRepository.GetQueryable(
                query => query
                    .Include(o => o.OpleidingPersonen)
                    .Include(o => o.OpleidingVereist)) // Vereiste opleiding expliciet laden
                .FirstOrDefaultAsync(o => o.Id == id);

            if (opleiding == null)
            {
                return NotFound();
            }

            return View(opleiding);
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
            var opleiding = await _unitOfWork.OpleidingRepository.GetByIdAsync(id);
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
                    // Prevent cyclische vereisten
                    if (IsCyclicPrerequisite(opleiding.Id, opleiding.OpleidingVereistId))
                    {
                        ModelState.AddModelError("OpleidingVereistId", "Cylische vereiste opleiding gedetecteerd.");
                    }
                    else
                    {
                        _unitOfWork.OpleidingRepository.Update(opleiding);
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

            var opleidingen = await _unitOfWork.OpleidingRepository.GetAllAsync();
            ViewBag.Opleidingen = opleidingen
                .Where(o => o.Id != id)
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

        #endregion
    }
}
