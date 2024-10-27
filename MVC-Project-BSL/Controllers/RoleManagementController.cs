using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MVC_Project_BSL.Data.UnitOfWork;
using MVC_Project_BSL.Models;
using MVC_Project_BSL.ViewModels;
using System.Diagnostics;
using System.Threading;

namespace MVC_Project_BSL.Controllers
{
    // [Authorize(Roles = "Beheerder")]
    public class RoleManagementController : Controller
    {
        private readonly RoleManager<IdentityRole<int>> _roleManager;
        private readonly UserManager<CustomUser> _userManager;
		private readonly IUnitOfWork _unitOfWork;
		private readonly MonitorService _monitorService;

		public RoleManagementController(MonitorService monitorService, RoleManager<IdentityRole<int>> roleManager, UserManager<CustomUser> userManager, IUnitOfWork unitOfWork)
        {
			_monitorService = monitorService;
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
				var user = await _userManager.FindByIdAsync(userId.ToString());
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
		private async Task RemoveOldRoleData(IEnumerable<string> userRoles, int userId)
		{
			Debug.WriteLine($"Removing old role data for user {userId} with roles: {string.Join(", ", userRoles)}");

			// Verkrijg de Kinderen van de gebruiker
			var deelnemers = await _unitOfWork.DeelnemerRepository.GetAllAsync();
			var gefilterdeDeelnemers = deelnemers.Where(k => k.KindId == userId).ToList();
			var monitoren = await _unitOfWork.MonitorRepository.GetAllAsync();
			var gefilterdeMonitoren = monitoren.Where(k => k.PersoonId == userId).ToList();

			// Verwijder monitor data als de rol niet langer "Monitor" of "Hoofdmonitor" is
			if (userRoles.Contains("Hoofdmonitor") || userRoles.Contains("Monitor"))
			{
				foreach (var monitor in gefilterdeMonitoren)
				{
					Debug.WriteLine($"Monitor gevonden voor user {userId} (Monitor ID: {monitor.Id}), verwijderen...");
					_unitOfWork.MonitorRepository.Delete(monitor);
				}
			}

			// Verwijder deelnemers alleen als de rol "Deelnemer" is
			if (userRoles.Contains("Deelnemer"))
			{
				foreach (var deelnemer in gefilterdeDeelnemers)
				{
					Debug.WriteLine($"Deelnemer gevonden voor user {userId} (Deelnemer ID: {deelnemer.Id}), verwijderen...");
					_unitOfWork.DeelnemerRepository.Delete(deelnemer);
				}
			}

			// Sla de wijzigingen op
			_unitOfWork.SaveChanges();
			Debug.WriteLine("Alle oude rol gegevens verwijderd voor de gebruiker.");
		}


		// Functie voor het toevoegen van nieuwe rolgegevens
		private async Task AddNewRoleData(string roleName, int userId, CustomUser user)
		{
			Debug.WriteLine($"Adding new role data for user {userId} with role {roleName}");

			// Check if the user already has a Monitor record in the database
			var monitor = await _unitOfWork.MonitorRepository.GetFirstOrDefaultAsync(m => m.PersoonId == userId);
			if (roleName == "Deelnemer")
			{
				if (monitor != null)
				{
					Debug.WriteLine($"Removing monitor data for user {userId} as they are now a Deelnemer...");
					_unitOfWork.MonitorRepository.Delete(monitor);
				}
				return; // Exit early as we don't need to do anything else for 'Deelnemer'
			}

			if (monitor == null)
			{
				// No existing Monitor record, so create a new one with the correct role
				Debug.WriteLine($"No existing monitor data found for user {userId}, adding new monitor data...");
				monitor = new Monitor
				{
					PersoonId = userId,
					Persoon = user,
					IsHoofdMonitor = roleName == "Hoofdmonitor"
				};

				await _unitOfWork.MonitorRepository.AddAsync(monitor);
				Debug.WriteLine($"New monitor data added for user {userId}");
			}
			else
			{
				// Update the existing monitor record's IsHoofdMonitor property
				Debug.WriteLine($"Updating existing monitor data for user {userId} to role {roleName}...");
				monitor.IsHoofdMonitor = roleName == "Hoofdmonitor";
				_unitOfWork.MonitorRepository.Update(monitor);
				Debug.WriteLine($"Monitor data updated for user {userId}");
			}

			// Save changes to the database
			_unitOfWork.SaveChanges();
		}
		[HttpPost]
		public async Task<IActionResult> MaakHoofdmonitor(int groepsreisId, int monitorId)
		{
			var result = await _monitorService.MaakHoofdmonitor(groepsreisId, monitorId);
			return result;
		}

		[HttpPost]
		public async Task<IActionResult> MaakGewoneMonitor(int groepsreisId, int monitorId)
		{
			var result = await _monitorService.MaakGewoneMonitor(groepsreisId, monitorId);
			return result;
		}

	}
}
