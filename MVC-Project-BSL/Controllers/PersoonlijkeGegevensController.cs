using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MVC_Project_BSL.Data.UnitOfWork;
using MVC_Project_BSL.Models;
using MVC_Project_BSL.ViewModels;

namespace MVC_Project_BSL.Controllers
{
    public class PersoonlijkeGegevensController : Controller
    {
        private readonly UserManager<CustomUser> _userManager;
        private readonly SignInManager<CustomUser> _signInManager;
        private readonly IUnitOfWork _unitOfWork;

        public PersoonlijkeGegevensController(UserManager<CustomUser> userManager, SignInManager<CustomUser> signInManager, IUnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _unitOfWork = unitOfWork;
        }

        // Index actie: Weergeven van persoonlijke gegevens van de gebruiker
        public async Task<IActionResult> Index()
        {
            // Haal de ingelogde gebruiker op
            var userId = int.Parse(_userManager.GetUserId(User));
            var user = await _unitOfWork.CustomUserRepository.GetByIdWithIncludesAsync(userId, u => u.Kinderen);

            if (user == null)
            {
                return NotFound("Gebruiker niet gevonden.");
            }

            // Map naar ViewModel
            var viewModel = new PersoonlijkeGegevensViewModel
            {
                Naam = user.Naam,
                Voornaam = user.Voornaam,
                Geboortedatum = user.Geboortedatum,
                Huisdokter = user.Huisdokter,
                TelefoonNummer = user.TelefoonNummer,
                RekeningNummer = user.RekeningNummer,
                IsActief = user.IsActief,
                Kinderen = user.Kinderen.Select(k => new KindGegevensViewModel
                {
                    Id = k.Id,
                    Naam = k.Naam,
                    Voornaam = k.Voornaam,
                    Geboortedatum = k.Geboortedatum,
                    Allergieën = k.Allergieën,
                    Medicatie = k.Medicatie,
                    PersoonId = userId
                }).ToList()
            };

            return View(viewModel);
        }

        // Edit actie: Weergeven van het formulier om persoonlijke gegevens te bewerken
        public async Task<IActionResult> Edit()
        {
            // Haal de ingelogde gebruiker op
            var userId = int.Parse(_userManager.GetUserId(User));
            var user = await _unitOfWork.CustomUserRepository.GetByIdWithIncludesAsync(userId, u => u.Kinderen);

            if (user == null)
            {
                return NotFound("Gebruiker niet gevonden.");
            }

            // Map naar ViewModel
            var viewModel = new PersoonlijkeGegevensViewModel
            {
                Naam = user.Naam,
                Voornaam = user.Voornaam,
                Geboortedatum = user.Geboortedatum,
                Huisdokter = user.Huisdokter,
                TelefoonNummer = user.TelefoonNummer,
                RekeningNummer = user.RekeningNummer,
                IsActief = user.IsActief,
                Kinderen = user.Kinderen.Select(k => new KindGegevensViewModel
                {
                    Id = k.Id,
                    Naam = k.Naam,
                    Voornaam = k.Voornaam,
                    Geboortedatum = k.Geboortedatum,
                    Allergieën = k.Allergieën,
                    Medicatie = k.Medicatie,
                    PersoonId = userId
                }).ToList()
            };

            return View(viewModel);
        }

        // Edit POST actie: Verwerken van het bewerken van persoonlijke gegevens van de gebruiker
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditGebruiker(PersoonlijkeGegevensViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Edit", model);
            }

            var userId = int.Parse(_userManager.GetUserId(User));
            var user = await _unitOfWork.CustomUserRepository.GetByIdAsync(userId);

            if (user == null)
            {
                return NotFound("Gebruiker niet gevonden.");
            }

            // Update de gebruiker gegevens
            user.Naam = model.Naam;
            user.Voornaam = model.Voornaam;
            user.Geboortedatum = model.Geboortedatum;
            user.Huisdokter = model.Huisdokter;
            user.TelefoonNummer = model.TelefoonNummer;
            user.RekeningNummer = model.RekeningNummer;

            // Alleen bijwerken van IsActief als de gebruiker een beheerder is
            if (User.IsInRole("Beheerder"))
            {
                user.IsActief = model.IsActief;
            }

            // Update gebruiker
            _unitOfWork.CustomUserRepository.Update(user);
            _unitOfWork.SaveChanges();

            // Vernieuw de gebruiker in de sessie
            await _signInManager.RefreshSignInAsync(user);

            TempData["SuccessMessage"] = "Gebruiker gegevens zijn correct opgeslagen!";
            return RedirectToAction(nameof(Index));
        }

        // Edit POST actie: Verwerken van het bewerken van gegevens van kinderen
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditKind(KindGegevensViewModel kindModel)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Er is iets mis met de ingevoerde gegevens.";
                return RedirectToAction(nameof(Index));
            }

            var userId = int.Parse(_userManager.GetUserId(User));
            var user = await _unitOfWork.CustomUserRepository.GetByIdWithIncludesAsync(userId, u => u.Kinderen);

            if (user == null)
            {
                return NotFound("Gebruiker niet gevonden.");
            }

            // Zoek het kind in de database
            var kind = await _unitOfWork.KindRepository.GetByIdAsync(kindModel.Id);
            if (kind == null || kind.PersoonId != userId)
            {
                return NotFound("Kind niet gevonden of behoort niet tot de gebruiker.");
            }

            // Update de kind gegevens
            kind.Naam = kindModel.Naam;
            kind.Voornaam = kindModel.Voornaam;
            kind.Geboortedatum = kindModel.Geboortedatum;
            kind.Allergieën = kindModel.Allergieën;
            kind.Medicatie = kindModel.Medicatie;

            // Update het kind via UnitOfWork
            _unitOfWork.KindRepository.Update(kind);
            _unitOfWork.SaveChanges();

            TempData["SuccessMessage"] = $"Gegevens van kind '{kind.Voornaam} {kind.Naam}' zijn correct opgeslagen!";
            return RedirectToAction(nameof(Index));
        }




        // Delete actie: Verwijder een kind van de gebruiker
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteChild(int id)
        {
            var userId = int.Parse(_userManager.GetUserId(User));
            var user = await _unitOfWork.CustomUserRepository.GetByIdWithIncludesAsync(userId, u => u.Kinderen);

            if (user == null)
            {
                return NotFound("Gebruiker niet gevonden.");
            }

            var kind = user.Kinderen.FirstOrDefault(k => k.Id == id);
            if (kind != null)
            {
                user.Kinderen.Remove(kind);
                _unitOfWork.KindRepository.Delete(kind); // Verwijder uit de KindRepository als Kinderen een aparte tabel is.
                _unitOfWork.SaveChanges();
                TempData["SuccessMessage"] = $"Kind '{kind.Voornaam} {kind.Naam}' is succesvol verwijderd.";
            }
            else
            {
                TempData["ErrorMessage"] = "Het kind kon niet gevonden worden.";
            }

            return RedirectToAction(nameof(Index));
        }

    }
}
