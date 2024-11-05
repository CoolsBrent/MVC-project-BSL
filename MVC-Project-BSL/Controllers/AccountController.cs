using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MVC_Project_BSL.Models;
using MVC_Project_BSL.ViewModels;
using System.Diagnostics;

namespace MVC_Project_BSL.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<CustomUser> _signInManager;
        private readonly UserManager<CustomUser> _userManager;

        public AccountController(SignInManager<CustomUser> signInManager, UserManager<CustomUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        // Registratie View
        public IActionResult Register()
        {
            return View();
        }

        // Afhandeling van de registratie
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new CustomUser
                {
                    UserName = model.Voornaam,
                    Email = model.Email,
                    Naam = model.Naam,
                    Voornaam = model.Voornaam,
                    Straat = model.Straat,
                    Huisnummer = model.Huisnummer,
                    Gemeente = model.Gemeente,
                    Postcode = model.Postcode,
                    Geboortedatum = model.Geboortedatum,
                    Huisdokter = model.Huisdokter,
                    TelefoonNummer = model.TelefoonNummer,
                    RekeningNummer = model.RekeningNummer,
                    IsActief = true
                };


                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    // Voeg gebruiker toe aan de rol 'Deelnemer'
                    await _userManager.AddToRoleAsync(user, "Deelnemer");

                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "Dashboard");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        // Login View
        public IActionResult Login()
        {
            return View();
        }

        // Afhandeling van de login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Probeer de gebruiker op te halen op basis van het e-mailadres
                var user = await _userManager.FindByEmailAsync(model.Email);

                if (user != null) // Zorg ervoor dat de gebruiker bestaat
                {
                    if (!user.IsActief)
                    {
                        ModelState.AddModelError(string.Empty, "Dit account is inactief en kan niet inloggen.");
                        return View(model);
                    }
                    // Gebruik de juiste overload van PasswordSignInAsync die een user-object accepteert
                    var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: false);

                    // Log het resultaat voor debugging
                    Debug.WriteLine($"SignIn result: {result.Succeeded}, User: {user.Email}");

                    if (result.Succeeded)
                    {
                        return RedirectToAction("Index", "Dashboard");
                    }
                    else if (result.IsLockedOut)
                    {
                        ModelState.AddModelError(string.Empty, "Account is vergrendeld. Probeer het later opnieuw.");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Ongeldig wachtwoord. Controleer uw wachtwoord en probeer het opnieuw.");
                    }
                }
                else
                {
                    // Optioneel: Geef een generieke foutmelding om te voorkomen dat je aangeeft dat de gebruiker niet bestaat
                    ModelState.AddModelError(string.Empty, "Ongeldige inlogpoging.");
                }
            }

            // Als we hier komen, is er een probleem met de invoer
            return View(model);
        }


        // Afhandeling van de logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

    }
}
