using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MVC_Project_BSL.Models;
using MVC_Project_BSL.ViewModels;
using System.Diagnostics;

namespace MVC_Project_BSL.Controllers
{
    /// <summary>
    /// Een controllerklasse voor het beheren van accountgerelateerde acties, waaronder gebruikersregistratie, inloggen en uitloggen.
    /// Verantwoordelijk voor authenticatie en initiële roltoewijzing van gebruikers.
    /// </summary>
    public class AccountController : Controller
    {
        #region Private Fields
        private readonly SignInManager<CustomUser> _signInManager;
        private readonly UserManager<CustomUser> _userManager;
        #endregion

        #region Constructor
        public AccountController(SignInManager<CustomUser> signInManager, UserManager<CustomUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }
        #endregion

        #region Registration Actions
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
        #endregion

        #region Login Actions
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
                var user = await _userManager.FindByEmailAsync(model.Email);

                if (user != null)
                {
                    if (!user.IsActief)
                    {
                        ModelState.AddModelError(string.Empty, "Dit account is inactief en kan niet inloggen.");
                        return View(model);
                    }

                    var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: false);

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
                    ModelState.AddModelError(string.Empty, "Ongeldige inlogpoging.");
                }
            }

            return View(model);
        }
        #endregion

        #region Logout Action
        // Afhandeling van de logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
        #endregion
    }
}
