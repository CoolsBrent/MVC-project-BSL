using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MVC_Project_BSL.Data.UnitOfWork;
using MVC_Project_BSL.Models;
using MVC_Project_BSL.ViewModels;
using System.Diagnostics;

namespace MVC_Project_BSL.Controllers
{
    /// <summary>
    /// De PersoonlijkeGegevensController biedt functionaliteit voor gebruikers om hun persoonlijke gegevens en die van hun kinderen te beheren.
    /// Gebruikers kunnen hun naam, geboortedatum, en contactinformatie bewerken en kinderen toevoegen, bewerken of verwijderen.
    /// Alle acties binnen deze controller vereisen dat de gebruiker is ingelogd.
    /// </summary>
    public class PersoonlijkeGegevensController : Controller
    {
        #region Private Fields
        private readonly UserManager<CustomUser> _userManager;
        private readonly SignInManager<CustomUser> _signInManager;
        private readonly IUnitOfWork _unitOfWork;
        #endregion

        #region Constructor
        public PersoonlijkeGegevensController(UserManager<CustomUser> userManager, SignInManager<CustomUser> signInManager, IUnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _unitOfWork = unitOfWork;
        }
        #endregion

        #region Index Action
        // Index actie: Weergeven van persoonlijke gegevens van de gebruiker
        public async Task<IActionResult> Index()
        {
            var userId = int.Parse(_userManager.GetUserId(User));
            var user = await _unitOfWork.CustomUserRepository.GetByIdWithIncludesAsync(userId, u => u.Kinderen);

            if (user == null)
            {
                return NotFound("Gebruiker niet gevonden.");
            }

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
                    Allergieen = string.IsNullOrEmpty(k.Allergieen) ? "Geen" : k.Allergieen,
                    Medicatie = string.IsNullOrEmpty(k.Medicatie) ? "Geen" : k.Medicatie,
                    PersoonId = userId
                }).ToList()
            };

            return View(viewModel);
        }
        #endregion

        #region Edit Gebruiker Actions
        // GET: Edit actie: Weergeven van het formulier om persoonlijke gegevens te bewerken
        public async Task<IActionResult> Edit()
        {
            var userId = int.Parse(_userManager.GetUserId(User));
            var user = await _unitOfWork.CustomUserRepository.GetByIdWithIncludesAsync(userId, u => u.Kinderen);

            if (user == null)
            {
                return NotFound("Gebruiker niet gevonden.");
            }

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
                    Allergieen = k.Allergieen,
                    Medicatie = k.Medicatie,
                    PersoonId = userId
                }).ToList()
            };

            return View(viewModel);
        }

        // POST: Edit Gebruiker actie
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

            user.Naam = model.Naam;
            user.Voornaam = model.Voornaam;
            user.Geboortedatum = model.Geboortedatum;
            user.Huisdokter = model.Huisdokter;
            user.TelefoonNummer = model.TelefoonNummer;
            user.RekeningNummer = model.RekeningNummer;

            if (User.IsInRole("Beheerder"))
            {
                user.IsActief = model.IsActief;
            }

            _unitOfWork.CustomUserRepository.Update(user);
            _unitOfWork.SaveChanges();

            await _signInManager.RefreshSignInAsync(user);

            TempData["SuccessMessage"] = "Gebruiker gegevens zijn correct opgeslagen!";
            return RedirectToAction(nameof(Index));
        }
		#endregion

		#region Add Kind Action
		// POST: Create actie om een kind toe te voegen
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> AddKind(KindGegevensViewModel kindModel)
		{
			// Debug: Controleer of het ModelState geldig is
			if (!ModelState.IsValid)
			{
				TempData["ErrorMessage"] = "Er is iets mis met de ingevoerde gegevens.";
				Debug.WriteLine($"[DEBUG] ModelState is not valid: {string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage))}");
				return RedirectToAction(nameof(Index));
			}

			// Debug: Log gebruiker ID ophalen
			var userId = int.Parse(_userManager.GetUserId(User));
			Debug.WriteLine($"[DEBUG] Retrieved UserId: {userId}");

			// Debug: Haal gebruiker op inclusief kinderen
			var user = await _unitOfWork.CustomUserRepository.GetByIdWithIncludesAsync(userId, u => u.Kinderen);
			if (user == null)
			{
				Debug.WriteLine($"[DEBUG] User with ID {userId} not found.");
				return NotFound("Gebruiker niet gevonden.");
			}

			Debug.WriteLine($"[DEBUG] User '{user.Naam}' has {user.Kinderen.Count} children before adding a new one.");

			// Maak het nieuwe kind aan
			var kind = new Kind
			{
				Naam = kindModel.Naam,
				Voornaam = kindModel.Voornaam,
				Geboortedatum = kindModel.Geboortedatum,
				Allergieen = string.IsNullOrWhiteSpace(kindModel.Allergieen) ? "Geen" : kindModel.Allergieen,
				Medicatie = string.IsNullOrWhiteSpace(kindModel.Medicatie) ? "Geen" : kindModel.Medicatie,
				PersoonId = userId
			};

			// Debug: Log details van het nieuwe kind
			Debug.WriteLine($"[DEBUG] Creating new child: {kind.Voornaam} {kind.Naam}, Geboortedatum: {kind.Geboortedatum}, Allergieën: {kind.Allergieen}, Medicatie: {kind.Medicatie}, PersoonId: {kind.PersoonId}");

			// Voeg het nieuwe kind toe
			try
			{
				await _unitOfWork.KindRepository.AddAsync(kind);

				// Debug: Log database wijzigingen
				Debug.WriteLine("[DEBUG] Attempting to save changes to database.");
				_unitOfWork.SaveChanges();

				Debug.WriteLine($"[DEBUG] New child '{kind.Voornaam} {kind.Naam}' added successfully.");
			}
			catch (Exception ex)
			{
				// Debug: Log eventuele fouten bij het opslaan
				Debug.WriteLine($"[ERROR] Exception while saving new child: {ex.Message}");
				TempData["ErrorMessage"] = "Er is een fout opgetreden bij het toevoegen van het kind.";
				return RedirectToAction(nameof(Index));
			}

			// Debug: Controleer aantal kinderen na toevoeging
			Debug.WriteLine($"[DEBUG] User '{user.Naam}' now has {user.Kinderen.Count} children after adding a new one.");

			TempData["SuccessMessage"] = $"Kind '{kind.Voornaam} {kind.Naam}' is succesvol toegevoegd!";
			return RedirectToAction(nameof(Index));
		}

		#endregion

		#region Edit Kind Action
		// POST: Edit Kind actie
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

            var kind = await _unitOfWork.KindRepository.GetByIdAsync(kindModel.Id);
            if (kind == null || kind.PersoonId != userId)
            {
                return NotFound("Kind niet gevonden of behoort niet tot de gebruiker.");
            }

            kind.Naam = kindModel.Naam;
            kind.Voornaam = kindModel.Voornaam;
            kind.Geboortedatum = kindModel.Geboortedatum;
            kind.Allergieen = string.IsNullOrEmpty(kindModel.Allergieen) ? "Geen" : kindModel.Allergieen;
            kind.Medicatie = string.IsNullOrEmpty(kindModel.Medicatie) ? "Geen" : kindModel.Medicatie;

            _unitOfWork.KindRepository.Update(kind);
            _unitOfWork.SaveChanges();

            TempData["SuccessMessage"] = $"Gegevens van kind '{kind.Voornaam} {kind.Naam}' zijn correct opgeslagen!";
            return RedirectToAction(nameof(Index));
        }
        #endregion

        #region Delete Kind Action
        // POST: Delete Kind actie
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
                _unitOfWork.KindRepository.Delete(kind);
                _unitOfWork.SaveChanges();
                TempData["SuccessMessage"] = $"Kind '{kind.Voornaam} {kind.Naam}' is succesvol verwijderd.";
            }
            else
            {
                TempData["ErrorMessage"] = "Het kind kon niet gevonden worden.";
            }

            return RedirectToAction(nameof(Index));
        }
        #endregion
    }
}
