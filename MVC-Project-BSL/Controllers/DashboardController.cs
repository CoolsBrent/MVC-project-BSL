using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MVC_Project_BSL.Data.UnitOfWork;
using MVC_Project_BSL.Models;
using MVC_Project_BSL.ViewModels;
using System.Security.Claims;

namespace MVC_Project_BSL.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<CustomUser> _userManager;

        public DashboardController(IUnitOfWork unitOfWork, UserManager<CustomUser> userManager)
        {

            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }
        // GET: Groepsreis
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(); // Als het gebruikers-ID niet beschikbaar is
            }

            var gebruiker = await _userManager.FindByIdAsync(userId);
            if (gebruiker == null)
            {
                return NotFound("Gebruiker niet gevonden."); // Als de gebruiker niet gevonden wordt
            }

            // Controleer of de gebruiker een beheerder is
            bool isBeheerder = await _userManager.IsInRoleAsync(gebruiker, "Beheerder");

            GroepsreisViewModel model;

            if (isBeheerder)
            {
                // Haal alle groepsreizen op voor beheerders
                var alleGroepsreizen = await _unitOfWork.GroepsreisRepository.GetAllAsync(query =>
                    query.Include(g => g.Deelnemers)
                         .ThenInclude(d => d.Kind)
                         .Include(g => g.Monitoren)
                         .Include(g => g.Bestemming)
                         .ThenInclude(b => b.Fotos)
                );

                model = new GroepsreisViewModel
                {
                    AlleGroepsReizen = alleGroepsreizen.ToList()
                };
            }
            else
            {
                // Haal alle kinderen op die aan de ingelogde gebruiker zijn gekoppeld
                var kinderen = await _unitOfWork.KindRepository.GetAllAsync();
                var gebruikersKinderen = kinderen.Where(k => k.PersoonId == gebruiker.Id).ToList();

                if (!gebruikersKinderen.Any())
                {
                    return NotFound("Geen kinderen gevonden voor deze gebruiker."); // Als er geen kinderen zijn gevonden
                }

                // Haal de groepsreizen op waarin de kinderen zijn ingeschreven
                var geboekteGroepsreizen = await _unitOfWork.GroepsreisRepository.GetAllAsync(query =>
                    query.Include(g => g.Deelnemers)
                         .ThenInclude(d => d.Kind)
                         .Include(g => g.Monitoren)
                         .Include(g => g.Bestemming)
                         .ThenInclude(b => b.Fotos)
                         .Where(g => g.Deelnemers.Any(d => gebruikersKinderen.Select(k => k.Id).Contains(d.KindId)))
                );

                // Haal de toekomstige groepsreizen op
                var toekomstigeGroepsreizen = await _unitOfWork.GroepsreisRepository.GetAllAsync(query =>
                    query.Include(g => g.Deelnemers)
                         .ThenInclude(d => d.Kind)
                         .Include(g => g.Monitoren)
                         .Include(g => g.Bestemming)
                         .ThenInclude(b => b.Fotos)
                         .Where(g => g.Begindatum > DateTime.Now)
                );

                model = new GroepsreisViewModel
                {
                    GeboekteGroepsReizen = (List<Groepsreis>)geboekteGroepsreizen,
                    ToekomstigeGroepsReizen = (List<Groepsreis>)toekomstigeGroepsreizen
                };
            }

            return View(model);
        }


    }
}
