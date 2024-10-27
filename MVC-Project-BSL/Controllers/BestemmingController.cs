using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MVC_Project_BSL.Data.UnitOfWork;
using MVC_Project_BSL.Models;
using MVC_Project_BSL.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MVC_Project_BSL.Controllers
{
    public class BestemmingController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public BestemmingController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // GET: Bestemming
        public async Task<IActionResult> Index()
        {
            var bestemmingen = await _unitOfWork.BestemmingRepository.GetQueryable(
            query => query.Include(b => b.Fotos))
                .ToListAsync();

            return View(bestemmingen);
        }

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

                // Verwerk de foto-upload
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

                                // Controleer of de map bestaat, zo niet, maak deze aan
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
                                // Log de fout en voeg een ModelState error toe
                                System.Diagnostics.Debug.WriteLine("FotoBestanden", $"Er is een fout opgetreden bij het uploaden van de foto {bestand.FileName}: {ex.Message}");
                                ModelState.AddModelError("FotoBestanden", $"Er is een fout opgetreden bij het uploaden van de foto {bestand.FileName}: {ex.Message}");
                                return View(model);
                            }
                        }
                    }
                }
                else
                {
                    // Voeg een ModelState error toe als er geen foto's zijn geüpload
                        System.Diagnostics.Debug.WriteLine("FotoBestanden", "Selecteer minstens één foto.");
                        ModelState.AddModelError("FotoBestanden", "Selecteer minstens één foto.");
                        System.Diagnostics.Debug.WriteLine($"Aantal FotoBestanden: {model.FotoBestanden?.Count ?? 0}");
                    return View(model);
                }

                await _unitOfWork.BestemmingRepository.AddAsync(bestemming);
                _unitOfWork.SaveChanges();
                return RedirectToAction(nameof(Index));
            }

            // Voeg deze code toe om de validatiefouten te loggen
            foreach (var key in ModelState.Keys)
            {
                var errors = ModelState[key].Errors;
                foreach (var error in errors)
                {
                    System.Diagnostics.Debug.WriteLine($"Key: {key}, Error: {error.ErrorMessage}");
                }
            }
            System.Diagnostics.Debug.WriteLine($"Aantal FotoBestanden: {model.FotoBestanden?.Count ?? 0}");
            System.Diagnostics.Debug.WriteLine("Modelstate invalid en returned naar view");
            return View(model);
        }

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


        private async Task LaadBestaandeFotos(BestemmingViewModel model, int id)
        {
            var bestemming = await _unitOfWork.BestemmingRepository.GetQueryable(
                query => query.Include(b => b.Fotos).Where(b => b.Id == id))
                .FirstOrDefaultAsync();

            model.BestaandeFotos = bestemming?.Fotos.ToList() ?? new List<Foto>();
        }

    
        // POST: Bestemming/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BestemmingViewModel model)
        {
            System.Diagnostics.Debug.WriteLine("Edit actie called");

            if (id != model.Id)
            {
                return NotFound();
            }

            // Controleer of het model geldig is
            if (!ModelState.IsValid)
            {
                System.Diagnostics.Debug.WriteLine("ModelState is not valid");

                // Log de ModelState-fouten
                foreach (var key in ModelState.Keys)
                {
                    var errors = ModelState[key].Errors;
                    foreach (var error in errors)
                    {
                        System.Diagnostics.Debug.WriteLine($"Key: {key}, Error: {error.ErrorMessage}");
                    }
                }

                // Laad de bestaande foto's opnieuw
                await LaadBestaandeFotos(model, id);
                return View(model);
            }

            // Haal de bestemming op inclusief de foto's
            var bestemming = await _unitOfWork.BestemmingRepository.GetQueryable(
                query => query.Include(b => b.Fotos))
                .FirstOrDefaultAsync(b => b.Id == id);

            if (bestemming == null)
            {
                return NotFound();
            }

            // Controleer of de foto's correct zijn geladen
            var aantalFotos = bestemming.Fotos.Count; // Voor debugging
            System.Diagnostics.Debug.WriteLine("Aantal foto's:" + aantalFotos);

            // Update de bestemmingsgegevens
            bestemming.Code = model.Code;
            bestemming.BestemmingsNaam = model.BestemmingsNaam;
            bestemming.Beschrijving = model.Beschrijving;
            bestemming.MinLeeftijd = model.MinLeeftijd;
            bestemming.MaxLeeftijd = model.MaxLeeftijd;

            // Verwijder geselecteerde foto's
            if (model.VerwijderFotosIds != null && model.VerwijderFotosIds.Any())
            {
                var fotosTeVerwijderen = bestemming.Fotos.Where(f => model.VerwijderFotosIds.Contains(f.Id)).ToList();

                foreach (var foto in fotosTeVerwijderen)
                {
                    // Verwijder het bestand van de schijf
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", foto.Naam);
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                    // Verwijder de foto uit de collectie en de database
                    bestemming.Fotos.Remove(foto);
                    _unitOfWork.FotoRepository.Delete(foto);
                }
            }

            // Controleer of er na het verwijderen nog bestaande foto's over zijn
            var heeftNogFotos = bestemming.Fotos.Any();

            // Controleer of er nieuwe foto's zijn geüpload
            var nieuweFotosGeupload = model.FotoBestanden != null && model.FotoBestanden.Count > 0;

            // Als er geen bestaande foto's zijn en er zijn geen nieuwe foto's geüpload, voeg een ModelState-fout toe
            if (!heeftNogFotos && !nieuweFotosGeupload)
            {
                ModelState.AddModelError("FotoBestanden", "Selecteer minstens één foto.");
                // Laad de bestaande foto's opnieuw
                await LaadBestaandeFotos(model, id);
                return View(model);
            }

            // Verwerk de nieuwe foto-upload
            if (nieuweFotosGeupload)
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

                            // Controleer of de map bestaat, zo niet, maak deze aan
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
                            // Log de fout en voeg een ModelState-fout toe
                            ModelState.AddModelError("FotoBestanden", $"Er is een fout opgetreden bij het uploaden van de foto {bestand.FileName}: {ex.Message}");
                            // Laad de bestaande foto's opnieuw
                            await LaadBestaandeFotos(model, id);
                            return View(model);
                        }
                    }
                }
            }

            // Update de bestemming in de database
            _unitOfWork.BestemmingRepository.Update(bestemming);
            _unitOfWork.SaveChanges();

            // Redirect naar de Index-pagina
            return RedirectToAction(nameof(Index));
        }


        // GET: Bestemming/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            System.Diagnostics.Debug.WriteLine("Delete actie called");

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
                // Verwijder de foto's van de schijf
                var fotos = await _unitOfWork.FotoRepository.GetAllAsync(f => f.Where(foto => foto.BestemmingId == id));
                foreach (var foto in fotos)
                {
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", foto.Naam);
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

                // Verwijder de bestemming
                _unitOfWork.BestemmingRepository.Delete(bestemming);
                _unitOfWork.SaveChanges();
            }
            return RedirectToAction(nameof(Index));
        }


        private async Task<bool> BestemmingExists(int id)
        {
            var bestemming = await _unitOfWork.BestemmingRepository.GetByIdAsync(id);
            return bestemming != null;
        }
    }
}
