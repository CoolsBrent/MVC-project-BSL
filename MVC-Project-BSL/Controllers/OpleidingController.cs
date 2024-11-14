using Microsoft.AspNetCore.Mvc;
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
                query => query.Include(o => o.OpleidingPersonen))
                .FirstOrDefaultAsync(o => o.Id == id);

            if (opleiding == null)
            {
                return NotFound();
            }

            return View(opleiding);
        }

        #endregion

        #region Create Actions

        // GET: Opleiding/Create
        [HttpGet]
        public IActionResult Create()
        {
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
                await _unitOfWork.OpleidingRepository.AddAsync(opleiding);
                _unitOfWork.SaveChanges();

                return RedirectToAction(nameof(Index));
            }

            // Laad eventuele benodigde data opnieuw als dat nodig is
            return View(opleiding);
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
                    _unitOfWork.OpleidingRepository.Update(opleiding);
                    _unitOfWork.SaveChanges();
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

                return RedirectToAction(nameof(Index));
            }

            return View(opleiding);
        }

        #endregion

        #region Delete Actions

        // GET: Opleiding/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var opleiding = await _unitOfWork.OpleidingRepository.GetQueryable(
                query => query.Include(o => o.OpleidingPersonen))
                .FirstOrDefaultAsync(o => o.Id == id);

            if (opleiding == null)
            {
                return NotFound();
            }

            return View(opleiding);
        }

        // POST: Opleiding/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var opleiding = await _unitOfWork.OpleidingRepository.GetByIdAsync(id);
            if (opleiding != null)
            {
                _unitOfWork.OpleidingRepository.Delete(opleiding);
                _unitOfWork.SaveChanges();
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
