using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MVC_Project_BSL.Data.UnitOfWork;
using MVC_Project_BSL.Models;
using MVC_Project_BSL.ViewModels;

namespace MVC_Project_BSL.Controllers
{
    /// <summary>
    /// Een controller voor het beheren van bestemmingen binnen de applicatie.
    /// Ondersteunt het weergeven, toevoegen, bewerken en verwijderen van bestemmingen, inclusief het beheren van foto-uploadfunctionaliteit.
    /// Gebruikt een Unit of Work-patroon voor database-interacties en biedt methoden voor het verwerken van bijbehorende foto's.
    /// </summary>
    public class BestemmingController : Controller
    {
        #region Private Fields
        private readonly IUnitOfWork _unitOfWork;
        #endregion

        #region Constructor
        public BestemmingController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        #endregion

        #region Index
        // GET: Bestemming
        public async Task<IActionResult> Index()
        {
            var bestemmingen = await _unitOfWork.BestemmingRepository.GetQueryable(
                query => query.Include(b => b.Fotos))
                .ToListAsync();

            return View(bestemmingen);
        }
        #endregion

        #region Details
        // GET: Bestemming/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var bestemming = await _unitOfWork.BestemmingRepository.GetAllAsync(
                query => query.Include(b => b.Fotos))
                .ContinueWith(t => t.Result.FirstOrDefault(b => b.Id == id));

            if (bestemming == null)
            {
                return NotFound();
            }
            return View(bestemming);
        }
        #endregion

        #region Create
        // GET: Bestemming/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Bestemming/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BestemmingViewModel model)
        {
            if (ModelState.IsValid)
            {
                System.Diagnostics.Debug.WriteLine("CREATE actie called ");
                var bestemming = new Bestemming
                {
                    Code = model.Code,
                    BestemmingsNaam = model.BestemmingsNaam,
                    Beschrijving = model.Beschrijving,
                    MinLeeftijd = model.MinLeeftijd,
                    MaxLeeftijd = model.MaxLeeftijd,
                    Fotos = new List<Foto>()
                };

                if (model.FotoBestanden != null && model.FotoBestanden.Count > 0)
                {
                    foreach (var bestand in model.FotoBestanden)
                    {
                        if (bestand.Length > 0)
                        {
                            try
                            {
                                var fileName = Path.GetFileName(bestand.FileName);
                                var uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
                                var filePath = Path.Combine(uploads, fileName);

                                if (!Directory.Exists(uploads))
                                {
                                    Directory.CreateDirectory(uploads);
                                }

                                using (var stream = new FileStream(filePath, FileMode.Create))
                                {
                                    await bestand.CopyToAsync(stream);
                                }

                                bestemming.Fotos.Add(new Foto
                                {
                                    Naam = fileName,
                                    Bestemming = bestemming
                                });
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine("FotoBestanden", $"Error uploading photo {bestand.FileName}: {ex.Message}");
                                ModelState.AddModelError("FotoBestanden", $"Error uploading photo {bestand.FileName}: {ex.Message}");
                                return View(model);
                            }
                        }
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("FotoBestanden", "Select at least one photo.");
                    ModelState.AddModelError("FotoBestanden", "Select at least one photo.");
                    return View(model);
                }

                await _unitOfWork.BestemmingRepository.AddAsync(bestemming);
                _unitOfWork.SaveChanges();
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }
        #endregion

        #region Edit
        // GET: Bestemming/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var bestemming = await _unitOfWork.BestemmingRepository.GetAllAsync(
                query => query.Include(b => b.Fotos))
                .ContinueWith(t => t.Result.FirstOrDefault(b => b.Id == id));

            if (bestemming == null)
            {
                return NotFound();
            }

            var model = new BestemmingViewModel
            {
                Id = bestemming.Id,
                Code = bestemming.Code,
                BestemmingsNaam = bestemming.BestemmingsNaam,
                Beschrijving = bestemming.Beschrijving,
                MinLeeftijd = bestemming.MinLeeftijd,
                MaxLeeftijd = bestemming.MaxLeeftijd,
                BestaandeFotos = bestemming.Fotos.ToList()
            };

            return View(model);
        }

        // POST: Bestemming/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BestemmingViewModel model)
        {
            System.Diagnostics.Debug.WriteLine("Edit action called");

            if (id != model.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                System.Diagnostics.Debug.WriteLine("ModelState is not valid");

                foreach (var key in ModelState.Keys)
                {
                    var errors = ModelState[key].Errors;
                    foreach (var error in errors)
                    {
                        System.Diagnostics.Debug.WriteLine($"Key: {key}, Error: {error.ErrorMessage}");
                    }
                }

                await LaadBestaandeFotos(model, id);
                return View(model);
            }

            var bestemming = await _unitOfWork.BestemmingRepository.GetQueryable(
                query => query.Include(b => b.Fotos))
                .FirstOrDefaultAsync(b => b.Id == id);

            if (bestemming == null)
            {
                return NotFound();
            }

            bestemming.Code = model.Code;
            bestemming.BestemmingsNaam = model.BestemmingsNaam;
            bestemming.Beschrijving = model.Beschrijving;
            bestemming.MinLeeftijd = model.MinLeeftijd;
            bestemming.MaxLeeftijd = model.MaxLeeftijd;

            if (model.VerwijderFotosIds != null && model.VerwijderFotosIds.Any())
            {
                var fotosTeVerwijderen = bestemming.Fotos.Where(f => model.VerwijderFotosIds.Contains(f.Id)).ToList();

                foreach (var foto in fotosTeVerwijderen)
                {
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", foto.Naam);
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                    bestemming.Fotos.Remove(foto);
                    _unitOfWork.FotoRepository.Delete(foto);
                }
            }

            if (!bestemming.Fotos.Any() && (model.FotoBestanden == null || model.FotoBestanden.Count == 0))
            {
                ModelState.AddModelError("FotoBestanden", "Selecteer minstens één foto.");
                await LaadBestaandeFotos(model, id);
                return View(model);
            }

            if (model.FotoBestanden != null)
            {
                foreach (var bestand in model.FotoBestanden)
                {
                    if (bestand.Length > 0)
                    {
                        try
                        {
                            var fileName = Path.GetFileNameWithoutExtension(bestand.FileName);
                            var extension = Path.GetExtension(bestand.FileName);
                            var uniqueFileName = $"{fileName}_{Guid.NewGuid()}{extension}";
                            var uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
                            var filePath = Path.Combine(uploads, uniqueFileName);

                            if (!Directory.Exists(uploads))
                            {
                                Directory.CreateDirectory(uploads);
                            }

                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                await bestand.CopyToAsync(stream);
                            }

                            bestemming.Fotos.Add(new Foto
                            {
                                Naam = uniqueFileName,
                                Bestemming = bestemming
                            });
                        }
                        catch (Exception ex)
                        {
                            ModelState.AddModelError("FotoBestanden", $"Error uploading photo {bestand.FileName}: {ex.Message}");
                            await LaadBestaandeFotos(model, id);
                            return View(model);
                        }
                    }
                }
            }

            _unitOfWork.BestemmingRepository.Update(bestemming);
            _unitOfWork.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        private async Task LaadBestaandeFotos(BestemmingViewModel model, int id)
        {
            var bestemming = await _unitOfWork.BestemmingRepository.GetQueryable(
                query => query.Include(b => b.Fotos).Where(b => b.Id == id))
                .FirstOrDefaultAsync();

            model.BestaandeFotos = bestemming?.Fotos.ToList() ?? new List<Foto>();
        }
        #endregion

        #region Delete
        // GET: Bestemming/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            System.Diagnostics.Debug.WriteLine("Delete action called");

            var bestemming = await _unitOfWork.BestemmingRepository.GetByIdAsync(id);
            if (bestemming == null)
            {
                return NotFound();
            }
            return View(bestemming);
        }

        // POST: Bestemming/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var bestemming = await _unitOfWork.BestemmingRepository.GetByIdAsync(id);
            if (bestemming != null)
            {
                var fotos = await _unitOfWork.FotoRepository.GetAllAsync(f => f.Where(foto => foto.BestemmingId == id));
                foreach (var foto in fotos)
                {
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", foto.Naam);
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

                _unitOfWork.BestemmingRepository.Delete(bestemming);
                _unitOfWork.SaveChanges();
            }
            return RedirectToAction(nameof(Index));
        }
        #endregion

        #region Helper Methods
        private async Task<bool> BestemmingExists(int id)
        {
            var bestemming = await _unitOfWork.BestemmingRepository.GetByIdAsync(id);
            return bestemming != null;
        }
        #endregion
    }
}