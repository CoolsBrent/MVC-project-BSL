using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MVC_Project_BSL.Data.UnitOfWork;
using MVC_Project_BSL.Models;
using MVC_Project_BSL.ViewModels;
using System.Diagnostics;

namespace MVC_Project_BSL.Controllers
{
    // [Authorize(Roles = "Beheerder")]
    public class RoleManagementController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<CustomUser> _userManager;
		private readonly IUnitOfWork _unitOfWork;

		public RoleManagementController(RoleManager<IdentityRole> roleManager, UserManager<CustomUser> userManager, IUnitOfWork unitOfWork)
        {
            _roleManager = roleManager;
            _userManager = userManager;
			_unitOfWork = unitOfWork;
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

		[HttpPost]
		public async Task<IActionResult> AssignRole(RoleManagementViewModel model)
		{
			Debug.WriteLine($"AssignRole started for user: {model.SelectedUserId} with role: {model.SelectedRole}");

			var userId = model.SelectedUserId; // Dit is de ID van de CustomUser
			var roleName = model.SelectedRole;
			var kindId = model.SelectedChildId;
			if (ModelState.IsValid)
			{
				var user = await _userManager.FindByIdAsync(userId);
				if (user == null)
				{
					Debug.WriteLine("Gebruiker niet gevonden.");
					ModelState.AddModelError(string.Empty, "Gebruiker niet gevonden.");
					return RedirectToAction("Index");
				}

				Debug.WriteLine($"User {userId} found. Fetching current roles...");

				// Haal de huidige rollen van de gebruiker op
				var userRoles = await _userManager.GetRolesAsync(user);

				Debug.WriteLine($"Current roles for user {userId}: {string.Join(", ", userRoles)}");

				// Verwijder de oude rolgegevens uit de juiste repository
				await RemoveOldRoleData(userRoles, userId);

				// Verwijder alle oude rollen behalve de nieuwe
				foreach (var role in userRoles)
				{
					if (role != roleName)
					{
						Debug.WriteLine($"Removing old role: {role} for user {userId}");

						var removeResult = await _userManager.RemoveFromRoleAsync(user, role);
						if (!removeResult.Succeeded)
						{
							Debug.WriteLine($"Error removing role {role} for user {userId}: {string.Join(", ", removeResult.Errors.Select(e => e.Description))}");
							ModelState.AddModelError(string.Empty, "Fout bij het verwijderen van de rol: " + string.Join(", ", removeResult.Errors.Select(e => e.Description)));
							return RedirectToAction("Index");
						}
					}
				}

				// Voeg de nieuwe rol toe
				if (!userRoles.Contains(roleName))
				{
					Debug.WriteLine($"Adding new role: {roleName} for user {userId}");

					var result = await _userManager.AddToRoleAsync(user, roleName);
					if (result.Succeeded)
					{
						Debug.WriteLine($"Role {roleName} successfully added to user {userId}. Adding role-specific data...");

						// Acties op basis van de nieuwe rol
						await AddNewRoleData(roleName, userId, user);

						Debug.WriteLine($"Role-specific data added for role {roleName} and user {userId}");
						return RedirectToAction("Index");
					}
					else
					{
						Debug.WriteLine($"Error adding role {roleName} for user {userId}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
						ModelState.AddModelError(string.Empty, "Fout bij het toevoegen van de rol: " + string.Join(", ", result.Errors.Select(e => e.Description)));
					}
				}
				else
				{
					Debug.WriteLine($"User {userId} already has the role {roleName}");
					ModelState.AddModelError(string.Empty, "Deze rol is al toegewezen aan de gebruiker.");
				}
			}
			else
			{
				Debug.WriteLine("ModelState is not valid.");
			}

			// Log modelstate errors
			foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
			{
				Debug.WriteLine($"ModelState Error: {error.ErrorMessage}");
			}

			return RedirectToAction("Index");
		}

		// Functie voor het verwijderen van oude rolgegevens
		private async Task RemoveOldRoleData(IEnumerable<string> userRoles, string userId)
		{
			Debug.WriteLine($"Removing old role data for user {userId} with roles: {string.Join(", ", userRoles)}");

			// Verkrijg de Kinderen van de gebruiker
			var kinderen = await _unitOfWork.KindRepository.GetAllAsync();
			var gefilterdeKinderen = kinderen.Where(k => k.PersoonId == userId).ToList();
			var monitoren = await _unitOfWork.MonitorRepository.GetAllAsync();
			var gefilterdeMonitoren = monitoren.Where(k => k.PersoonId == userId).ToList();

			if (userRoles.Contains("Monitor"))
			{
				
				foreach (var monitor in gefilterdeMonitoren)
				{
					Debug.WriteLine($"Deelnemer gevonden voor user {userId} (Kind ID: {monitor.Id}), verwijderen...");
					_unitOfWork.MonitorRepository.Delete(monitor);
				}
			}
            if (userRoles.Contains("Deelnemer"))
            {
				foreach (var kind in gefilterdeKinderen)
				{
					Debug.WriteLine($"Deelnemer gevonden voor user {userId} (Kind ID: {kind.Id}), verwijderen...");
					_unitOfWork.KindRepository.Delete(kind);
				}
			}

            // Verwijder alle Kinderen die aan deze gebruiker zijn gekoppeld
           

			// Sla de wijzigingen op
			 _unitOfWork.SaveChanges();
			Debug.WriteLine("Alle deelnemers verwijderd voor de gebruiker.");
		}

		// Functie voor het toevoegen van nieuwe rolgegevens
		private async Task AddNewRoleData(string roleName, string userId, CustomUser user)
		{
			Debug.WriteLine($"Adding new role data for user {userId} with role {roleName}");

			if (roleName == "Monitor")
			{
				var monitorExists = await _unitOfWork.MonitorRepository.AnyAsync(m => m.PersoonId == userId);
				if (!monitorExists)
				{
					Debug.WriteLine($"No existing monitor data found for user {userId}, adding new monitor data...");
					var monitor = new Models.Monitor
					{
						Id = Guid.NewGuid().ToString(),
						PersoonId = userId,
						Persoon = user,
						IsHoofdMonitor = false
					};

					await _unitOfWork.MonitorRepository.AddAsync(monitor);
					_unitOfWork.SaveChanges();
					Debug.WriteLine($"New monitor data added for user {userId}");
				}
				else
				{
					Debug.WriteLine($"Monitor data already exists for user {userId}");
				}
			}
			else if (roleName == "Deelnemer")
			{
				var deelnemerExists = await _unitOfWork.KindRepository.AnyAsync(d => d.PersoonId == userId);
				if (!deelnemerExists)
				{
					Debug.WriteLine($"No existing deelnemer data found for user {userId}, adding new deelnemer data...");
					var kind = new Kind
					{
						Naam = user.Naam,
						Voornaam = user.Voornaam,
						Geboortedatum = user.Geboortedatum,
						Allergieen = "",
						Medicatie = "",
						PersoonId = userId,
						Persoon = user
					};

					await _unitOfWork.KindRepository.AddAsync(kind);
					_unitOfWork.SaveChanges();
					Debug.WriteLine($"New deelnemer data added for user {userId}");
				}
				else
				{
					Debug.WriteLine($"Deelnemer data already exists for user {userId}");
				}
			}
		}


	}
}
