using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MVC_Project_BSL.Data.UnitOfWork;
using MVC_Project_BSL.Models;
using MVC_Project_BSL.ViewModels;

namespace MVC_Project_BSL.Controllers
{
    /// <summary>
    /// Een controller voor het beheren van activiteiten.
    /// Met deze controller kunnen activiteiten worden bekeken, toegevoegd, bewerkt en verwijderd.
    /// </summary>
    public class ActiviteitController : Controller
    {
        #region private fields
        private readonly IUnitOfWork _unitOfWork;
        #endregion

        #region Constructor
        public ActiviteitController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        #endregion

        #region Helper Methods

        // Helper method to fetch Groepsreizen from the repository and convert to SelectListItem
        private async Task<IEnumerable<SelectListItem>> GetGroepsreisOptionsAsync()
        {
            // Ensure that Bestemming is included in the query
            var groepsreizen = await _unitOfWork.GroepsreisRepository
                .GetQueryable()
                .Include(gr => gr.Bestemming)  // Include Bestemming property
                .ToListAsync();

            return groepsreizen.Select(gr => new SelectListItem
            {
                Value = gr.Id.ToString(),  // The Groepsreis ID
                Text = gr.Bestemming?.BestemmingsNaam ?? "Onbekende bestemming"  // Display BestemmingsNaam if available
            });
        }

        #endregion

        #region Index Action

        // GET: Activiteit
        public async Task<IActionResult> Index()
        {
            // Use UnitOfWork to retrieve all activiteiten with related Programma data
            var activiteiten = await _unitOfWork.ActiviteitRepository.GetAllAsync(
                query => query.Include(a => a.Programmas)
                .ThenInclude(p => p.Groepsreis)
                .ThenInclude(gr => gr.Bestemming)
            );

            // Map to ViewModel
            var activiteitViewModels = activiteiten.Select(a => new ActiviteitViewModel
            {
                Id = a.Id,
                Naam = a.Naam,
                Beschrijving = a.Beschrijving,
                Programmas = a.Programmas.Select(p => new ProgrammaViewModel
                {
                    Id = p.Id,
                    ActiviteitId = p.ActiviteitId,
                    GroepsreisId = p.GroepsreisId,
                    GroepsreisNaam = p.Groepsreis?.Bestemming?.BestemmingsNaam ?? "Onbekende bestemming"
                }).ToList()
            }).ToList();

            return View(activiteitViewModels);
        }

        #endregion

        #region Create Action

        // GET: Activiteit/Create
        public async Task<IActionResult> Create()
        {
            var viewModel = new ActiviteitViewModel
            {
                GroepsreisOptions = await GetGroepsreisOptionsAsync() // Load Groepsreizen for selection
            };
            return View(viewModel);
        }

        // POST: Activiteit/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ActiviteitViewModel model)
        {
            if (ModelState.IsValid)
            {
                var activiteit = new Activiteit
                {
                    Naam = model.Naam,
                    Beschrijving = model.Beschrijving
                };

                await _unitOfWork.ActiviteitRepository.AddAsync(activiteit);
                _unitOfWork.SaveChanges();

                // Koppel geselecteerde Groepsreizen aan de Activiteit
                foreach (var groepsreisId in model.SelectedGroepsreisIds)
                {
                    var programma = new Programma
                    {
                        ActiviteitId = activiteit.Id,
                        GroepsreisId = groepsreisId
                    };
                    await _unitOfWork.ProgrammaRepository.AddAsync(programma);
                }

                _unitOfWork.SaveChanges();
                return RedirectToAction(nameof(Index));
            }

            // Laad Groepsreis-opties opnieuw als er een validatiefout optreedt
            model.GroepsreisOptions = await GetGroepsreisOptionsAsync();
            return View(model);
        }

        #endregion

        #region Edit Action

        // GET: Activiteit/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var activiteit = await _unitOfWork.ActiviteitRepository.GetByIdWithIncludesAsync(id.Value, a => a.Programmas);
            if (activiteit == null)
            {
                return NotFound();
            }

            var viewModel = new ActiviteitViewModel
            {
                Id = activiteit.Id,
                Naam = activiteit.Naam,
                Beschrijving = activiteit.Beschrijving,

                // Zet SelectedGroepsreisIds als een lijst van alle gekoppelde GroepsreisIds
                SelectedGroepsreisIds = activiteit.Programmas.Select(p => p.GroepsreisId).ToList(),

                // Laad alle Groepsreis opties
                GroepsreisOptions = await GetGroepsreisOptionsAsync()
            };

            return View(viewModel);
        }

        // POST: Activiteit/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ActiviteitViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var activiteit = await _unitOfWork.ActiviteitRepository.GetByIdAsync(id);
                if (activiteit == null)
                {
                    return NotFound();
                }

                activiteit.Naam = model.Naam;
                activiteit.Beschrijving = model.Beschrijving;
                _unitOfWork.ActiviteitRepository.Update(activiteit);
                _unitOfWork.SaveChanges();

                // Verwijder bestaande Programma-koppelingen
                var bestaandeProgrammas = await _unitOfWork.ProgrammaRepository.GetAllAsync(
                    p => p.Where(pr => pr.ActiviteitId == activiteit.Id)
                );

                foreach (var programma in bestaandeProgrammas)
                {
                    _unitOfWork.ProgrammaRepository.Delete(programma);
                }

                // Voeg nieuwe Groepsreis-koppelingen toe
                foreach (var groepsreisId in model.SelectedGroepsreisIds)
                {
                    var programma = new Programma
                    {
                        ActiviteitId = activiteit.Id,
                        GroepsreisId = groepsreisId
                    };
                    await _unitOfWork.ProgrammaRepository.AddAsync(programma);
                }

                _unitOfWork.SaveChanges();
                return RedirectToAction(nameof(Index));
            }

            model.GroepsreisOptions = await GetGroepsreisOptionsAsync();
            return View(model);
        }

        #endregion

        #region Delete Action

        // GET: Activiteit/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Haal de activiteit op inclusief gekoppelde Programma's
            var activiteit = await _unitOfWork.ActiviteitRepository.GetByIdWithIncludesAsync(id.Value, a => a.Programmas);

            if (activiteit == null)
            {
                return NotFound();
            }

            // Haal de gekoppelde Groepsreizen inclusief Bestemming op
            var groepsreisIds = activiteit.Programmas.Select(p => p.GroepsreisId).ToList();
            var gekoppeldeGroepsreizen = await _unitOfWork.GroepsreisRepository.GetQueryable()
                .Where(gr => groepsreisIds.Contains(gr.Id))
                .Include(gr => gr.Bestemming) // Zorg ervoor dat Bestemming is geladen
                .ToListAsync();

            // Bouw de ProgrammaViewModels op met de juiste GroepsreisNaam
            var programmaViewModels = activiteit.Programmas.Select(p => new ProgrammaViewModel
            {
                Id = p.Id,
                ActiviteitId = p.ActiviteitId,
                GroepsreisId = p.GroepsreisId,
                GroepsreisNaam = gekoppeldeGroepsreizen
                    .FirstOrDefault(gr => gr.Id == p.GroepsreisId)?.Bestemming?.BestemmingsNaam ?? "Onbekende bestemming"
            }).ToList();

            // Maak een ViewModel voor de activiteit en voeg Programma's toe
            var viewModel = new ActiviteitViewModel
            {
                Id = activiteit.Id,
                Naam = activiteit.Naam,
                Beschrijving = activiteit.Beschrijving,
                Programmas = programmaViewModels
            };

            return View(viewModel);
        }

        // POST: Activiteit/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var activiteit = await _unitOfWork.ActiviteitRepository.GetByIdAsync(id);
            if (activiteit != null)
            {
                // Remove associated Programmas
                var programmas = await _unitOfWork.ProgrammaRepository.GetAllAsync(
                    p => p.Where(pr => pr.ActiviteitId == id)
                );

                foreach (var programma in programmas)
                {
                    _unitOfWork.ProgrammaRepository.Delete(programma);
                }

                _unitOfWork.ActiviteitRepository.Delete(activiteit);
                _unitOfWork.SaveChanges();
            }
            return RedirectToAction(nameof(Index));
        }

        #endregion
    }
}