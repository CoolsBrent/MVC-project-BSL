using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MVC_Project_BSL.Models;
using MVC_Project_BSL.ViewModels;

namespace MVC_Project_BSL.Controllers
{
    // [Authorize(Roles = "Beheerder")]
    public class RoleManagementController : Controller
    {
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly UserManager<CustomUser> _userManager;

        public RoleManagementController(RoleManager<ApplicationRole> roleManager, UserManager<CustomUser> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var users = _userManager.Users.ToList();
            var userRolesViewModels = new List<UserRolesViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userRolesViewModels.Add(new UserRolesViewModel
                {
                    User = user,
                    Roles = roles.ToList()
                });
            }

            var viewModel = new RoleManagementViewModel
            {
                Roles = _roleManager.Roles.ToList(),
                Users = userRolesViewModels
            };

            return View(viewModel);
        }

        // Toewijzen van een rol aan een gebruiker
        [HttpPost]
        public async Task<IActionResult> AssignRole(RoleManagementViewModel model)
        {
            var userId = model.SelectedUserId;
            var roleName = model.SelectedRole;

            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "Gebruiker niet gevonden.");
                    return RedirectToAction("Index");
                }

                // Haal de huidige rollen van de gebruiker op
                var userRoles = await _userManager.GetRolesAsync(user);

                // Verwijder de oude rol (bijvoorbeeld "Deelnemer") als deze bestaat
                foreach (var role in userRoles)
                {
                    if (role != roleName) // Zorg ervoor dat je niet de nieuwe rol verwijdert
                    {
                        var removeResult = await _userManager.RemoveFromRoleAsync(user, role);
                        if (!removeResult.Succeeded)
                        {
                            ModelState.AddModelError(string.Empty, "Fout bij het verwijderen van de rol: " + string.Join(", ", removeResult.Errors.Select(e => e.Description)));
                            return RedirectToAction("Index");
                        }
                    }
                }

                // Voeg de nieuwe rol toe
                if (!userRoles.Contains(roleName))
                {
                    var result = await _userManager.AddToRoleAsync(user, roleName);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Fout bij het toevoegen van de rol: " + string.Join(", ", result.Errors.Select(e => e.Description)));
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Deze rol is al toegewezen aan de gebruiker.");
                }
            }

            // Log modelstate errors
            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                Console.WriteLine($"ModelState Error: {error.ErrorMessage}");
            }

            return RedirectToAction("Index");
        }

    }
}
