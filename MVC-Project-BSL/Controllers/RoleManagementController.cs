using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MVC_Project_BSL.Data.UnitOfWork;
using MVC_Project_BSL.Models;
using MVC_Project_BSL.Services;
using MVC_Project_BSL.ViewModels;
using System.Diagnostics;

namespace MVC_Project_BSL.Controllers
{
    /// <summary>
    /// De RoleManagementController biedt functionaliteiten voor het beheren van gebruikersrollen en toegangsrechten.
    /// Alleen beheerders kunnen deze acties uitvoeren. De controller omvat weergaven voor het toewijzen, aanpassen,
    /// en verwijderen van rollen, inclusief helperfuncties voor rolafhankelijke gegevensbeheer en specifieke
    /// gebruikersacties zoals deactiveren en verwijderen van gebruikers.
    /// </summary>
    [Authorize(Roles = "Beheerder")]
    public class RoleManagementController : Controller
    {
        #region Private Fields
        private readonly RoleManager<IdentityRole<int>> _roleManager;
        private readonly UserManager<CustomUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly MonitorService _monitorService;
        private readonly SignInManager<CustomUser> _signInManager;
        #endregion

        #region Constructor
        public RoleManagementController(MonitorService monitorService, SignInManager<CustomUser> signInManager, RoleManager<IdentityRole<int>> roleManager, UserManager<CustomUser> userManager, IUnitOfWork unitOfWork)
        {
            _monitorService = monitorService;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _userManager = userManager;
            _unitOfWork = unitOfWork;
        }
        #endregion

        #region Role Management Views
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
        #endregion

        #region Role Assignment
        [HttpPost]
        public async Task<IActionResult> AssignRole(RoleManagementViewModel model)
        {
            Debug.WriteLine($"AssignRole started for user: {model.SelectedUserId} with role: {model.SelectedRole}");

            var userId = model.SelectedUserId;
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
                var userRoles = await _userManager.GetRolesAsync(user);

                await RemoveOldRoleData(userRoles, userId);
                foreach (var role in userRoles)
                {
                    if (role != roleName)
                    {
                        Debug.WriteLine($"Removing old role: {role} for user {userId}");
                        var removeResult = await _userManager.RemoveFromRoleAsync(user, role);
                        if (!removeResult.Succeeded)
                        {
                            ModelState.AddModelError(string.Empty, $"Fout bij het verwijderen van de rol: {string.Join(", ", removeResult.Errors.Select(e => e.Description))}");
                            return RedirectToAction("Index");
                        }
                    }
                }

                if (!userRoles.Contains(roleName))
                {
                    Debug.WriteLine($"Adding new role: {roleName} for user {userId}");
                    var result = await _userManager.AddToRoleAsync(user, roleName);
                    if (result.Succeeded)
                    {
                        await AddNewRoleData(roleName, userId, user);
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, $"Fout bij het toevoegen van de rol: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Deze rol is al toegewezen aan de gebruiker.");
                }
            }

            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                Debug.WriteLine($"ModelState Error: {error.ErrorMessage}");
            }

            return RedirectToAction("Index");
        }
        #endregion

        #region Role Data Modification Helpers
        private async Task RemoveOldRoleData(IEnumerable<string> userRoles, int userId)
        {
            Debug.WriteLine($"Removing old role data for user {userId} with roles: {string.Join(", ", userRoles)}");

            var deelnemers = await _unitOfWork.DeelnemerRepository.GetAllAsync();
            var gefilterdeDeelnemers = deelnemers.Where(k => k.KindId == userId).ToList();
            var monitoren = await _unitOfWork.MonitorRepository.GetAllAsync();
            var gefilterdeMonitoren = monitoren.Where(k => k.PersoonId == userId).ToList();

            if (userRoles.Contains("Hoofdmonitor") || userRoles.Contains("Monitor"))
            {
                foreach (var monitor in gefilterdeMonitoren)
                {
                    Debug.WriteLine($"Monitor gevonden voor user {userId} (Monitor ID: {monitor.Id}), verwijderen...");
                    _unitOfWork.MonitorRepository.Delete(monitor);
                }
            }

            if (userRoles.Contains("Deelnemer"))
            {
                foreach (var deelnemer in gefilterdeDeelnemers)
                {
                    Debug.WriteLine($"Deelnemer gevonden voor user {userId} (Deelnemer ID: {deelnemer.Id}), verwijderen...");
                    _unitOfWork.DeelnemerRepository.Delete(deelnemer);
                }
            }

            _unitOfWork.SaveChanges();
            Debug.WriteLine("Alle oude rol gegevens verwijderd voor de gebruiker.");
        }

        private async Task AddNewRoleData(string roleName, int userId, CustomUser user)
        {
            Debug.WriteLine($"Adding new role data for user {userId} with role {roleName}");

            var monitor = await _unitOfWork.MonitorRepository.GetFirstOrDefaultAsync(m => m.PersoonId == userId);
            if (roleName == "Deelnemer")
            {
                if (monitor != null)
                {
                    _unitOfWork.MonitorRepository.Delete(monitor);
                }
                return;
            }

            if (monitor == null)
            {
                monitor = new Models.Monitor
                {
                    PersoonId = userId,
                    Persoon = user,
                    IsHoofdMonitor = roleName == "Hoofdmonitor"
                };
                await _unitOfWork.MonitorRepository.AddAsync(monitor);
            }
            else
            {
                monitor.IsHoofdMonitor = roleName == "Hoofdmonitor";
                _unitOfWork.MonitorRepository.Update(monitor);
            }

            _unitOfWork.SaveChanges();
        }
        #endregion

        #region Role Actions for Monitor Management
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
        #endregion

        #region User Management Actions
        [HttpPost]
        public async Task<IActionResult> DeactivateUser(int userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user != null)
            {
                user.IsActief = false;
                await _userManager.UpdateAsync(user);
                _unitOfWork.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(int userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user != null)
            {
                await _userManager.DeleteAsync(user);
                _unitOfWork.SaveChanges();
            }
            return RedirectToAction("Index");
        }
        #endregion

        #region Edit User Data Actions
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _unitOfWork.CustomUserRepository.GetByIdAsync(id);

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
            };

            return View("EditGebruiker", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditGebruiker(PersoonlijkeGegevensViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("EditGebruiker", model);
            }

            var user = await _unitOfWork.CustomUserRepository.GetByIdAsync(model.Id);

            if (user == null)
            {
                return NotFound("Gebruiker niet gevonden.");
            }

            user.Id = model.Id;
            user.Naam = model.Naam;
            user.Voornaam = model.Voornaam;
            user.Geboortedatum = model.Geboortedatum;
            user.Huisdokter = model.Huisdokter;
            user.TelefoonNummer = model.TelefoonNummer;
            user.RekeningNummer = model.RekeningNummer;
            user.IsActief = model.IsActief;

            _unitOfWork.CustomUserRepository.Update(user);
            _unitOfWork.SaveChanges();

            TempData["SuccessMessage"] = "Gebruiker gegevens zijn correct opgeslagen!";
            return RedirectToAction(nameof(Index));
        }
        #endregion
    }
}