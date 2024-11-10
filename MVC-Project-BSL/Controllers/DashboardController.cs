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
    /// <summary>
    /// Een controller voor het beheren en weergeven van het dashboard, aangepast aan de rol van de ingelogde gebruiker.
    /// Voor beheerders, monitoren en deelnemers worden relevante groepsreizen en bestemmingen weergegeven, 
    /// waarbij verschillende filters en archiveringsstatussen worden toegepast.
    /// </summary>
    [Authorize]
    public class DashboardController : Controller
    {
        #region Private Fields
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<CustomUser> _userManager;
        #endregion

        #region Constructor
        public DashboardController(IUnitOfWork unitOfWork, UserManager<CustomUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }
        #endregion

        #region Index
        // GET: Groepsreis
        public async Task<IActionResult> Index(string bestemming)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var gebruiker = await _userManager.FindByIdAsync(userId);
            if (gebruiker == null)
            {
                return NotFound("Gebruiker niet gevonden.");
            }

            bool isBeheerder = await _userManager.IsInRoleAsync(gebruiker, "Beheerder");
            bool isMonitor = await _userManager.IsInRoleAsync(gebruiker, "Monitor");

            GroepsreisViewModel model;

            if (isBeheerder)
            {
                model = await LoadGroepsreizenForBeheerder(bestemming);
            }
            else if (isMonitor)
            {
                model = await LoadGroepsreizenForMonitor(gebruiker.Id, bestemming);
            }
            else
            {
                model = await LoadGroepsreizenForUser(gebruiker.Id, bestemming);
            }

            return View(model);
        }
        #endregion

        #region Private Methods

        private async Task<GroepsreisViewModel> LoadGroepsreizenForBeheerder(string bestemming)
        {
            var alleGroepsreizen = await _unitOfWork.GroepsreisRepository.GetAllAsync(query =>
                query.Include(g => g.Deelnemers)
                     .ThenInclude(d => d.Kind)
                     .Include(g => g.Monitoren)
                     .Include(g => g.Bestemming)
                     .ThenInclude(b => b.Fotos)
                     .Where(g => !g.IsArchived));

            var gearchriveerdeGroepsreizen = await _unitOfWork.GroepsreisRepository.GetAllAsync(query =>
                query.Include(g => g.Deelnemers)
                     .ThenInclude(d => d.Kind)
                     .Include(g => g.Monitoren)
                     .Include(g => g.Bestemming)
                     .ThenInclude(b => b.Fotos)
                     .Where(g => g.IsArchived));

            if (!string.IsNullOrEmpty(bestemming))
            {
                alleGroepsreizen = alleGroepsreizen.Where(g => g.Bestemming.BestemmingsNaam == bestemming);
                gearchriveerdeGroepsreizen = gearchriveerdeGroepsreizen.Where(g => g.Bestemming.BestemmingsNaam == bestemming);
            }

            alleGroepsreizen = alleGroepsreizen.OrderBy(g => g.Begindatum);
            var alleBestemmingen = await _unitOfWork.BestemmingRepository.GetAllAsync();

            return new GroepsreisViewModel
            {
                AlleGroepsReizen = alleGroepsreizen.ToList(),
                AlleBestemmingen = alleBestemmingen.ToList(),
                GearchiveerdeGroepsreizen = gearchriveerdeGroepsreizen.ToList()
            };
        }

        private async Task<GroepsreisViewModel> LoadGroepsreizenForMonitor(int userId, string bestemming)
        {
            var geboekteGroepsreizen = await _unitOfWork.GroepsreisRepository.GetAllAsync(query =>
                query.Include(g => g.Deelnemers)
                     .ThenInclude(d => d.Kind)
                     .Include(g => g.Monitoren)
                     .Include(g => g.Bestemming)
                     .ThenInclude(b => b.Fotos)
                     .Where(g => g.Monitoren.Any(m => m.Monitor.PersoonId == userId) && !g.IsArchived));

            var toekomstigeGroepsreizen = await _unitOfWork.GroepsreisRepository.GetAllAsync(query =>
                query.Include(g => g.Deelnemers)
                     .ThenInclude(d => d.Kind)
                     .Include(g => g.Monitoren)
                     .Include(g => g.Bestemming)
                     .ThenInclude(b => b.Fotos)
                     .Where(g => g.Begindatum > DateTime.Now && !g.IsArchived));

            if (!string.IsNullOrEmpty(bestemming))
            {
                geboekteGroepsreizen = geboekteGroepsreizen.Where(g => g.Bestemming.BestemmingsNaam == bestemming).ToList();
                toekomstigeGroepsreizen = toekomstigeGroepsreizen.Where(g => g.Bestemming.BestemmingsNaam == bestemming).ToList();
            }

            geboekteGroepsreizen = geboekteGroepsreizen.OrderBy(g => g.Begindatum).ToList();
            toekomstigeGroepsreizen = toekomstigeGroepsreizen.OrderBy(g => g.Begindatum).ToList();

            var alleBestemmingen = await _unitOfWork.BestemmingRepository.GetAllAsync();

            return new GroepsreisViewModel
            {
                GeboekteGroepsReizen = geboekteGroepsreizen.ToList(),
                ToekomstigeGroepsReizen = toekomstigeGroepsreizen.ToList(),
                AlleBestemmingen = alleBestemmingen.ToList()
            };
        }

        private async Task<GroepsreisViewModel> LoadGroepsreizenForUser(int userId, string bestemming)
        {
            var kinderen = await _unitOfWork.KindRepository.GetAllAsync();
            var gebruikersKinderen = kinderen.Where(k => k.PersoonId == userId).ToList();

            if (!gebruikersKinderen.Any())
            {
                var toekomstigeGroepsreizen = await _unitOfWork.GroepsreisRepository.GetAllAsync(query =>
                    query.Include(g => g.Deelnemers)
                         .ThenInclude(d => d.Kind)
                         .Include(g => g.Monitoren)
                         .Include(g => g.Bestemming)
                         .ThenInclude(b => b.Fotos)
                         .Where(g => g.Begindatum > DateTime.Now && !g.IsArchived));

                if (!string.IsNullOrEmpty(bestemming))
                {
                    toekomstigeGroepsreizen = toekomstigeGroepsreizen.Where(g => g.Bestemming.BestemmingsNaam == bestemming);
                }

                toekomstigeGroepsreizen = toekomstigeGroepsreizen.OrderBy(g => g.Begindatum);

                var alleBestemmingen = await _unitOfWork.BestemmingRepository.GetAllAsync();

                return new GroepsreisViewModel
                {
                    ToekomstigeGroepsReizen = toekomstigeGroepsreizen.ToList(),
                    AlleBestemmingen = alleBestemmingen.ToList()
                };
            }
            else
            {
                var geboekteGroepsreizen = await _unitOfWork.GroepsreisRepository.GetAllAsync(query =>
                    query.Include(g => g.Deelnemers)
                         .ThenInclude(d => d.Kind)
                         .Include(g => g.Monitoren)
                         .Include(g => g.Bestemming)
                         .ThenInclude(b => b.Fotos)
                         .Where(g => g.Deelnemers.Any(d => gebruikersKinderen.Select(k => k.Id).Contains(d.KindId) && !g.IsArchived)));

                var toekomstigeGroepsreizen = await _unitOfWork.GroepsreisRepository.GetAllAsync(query =>
                    query.Include(g => g.Deelnemers)
                         .ThenInclude(d => d.Kind)
                         .Include(g => g.Monitoren)
                         .Include(g => g.Bestemming)
                         .ThenInclude(b => b.Fotos)
                         .Where(g => g.Begindatum > DateTime.Now && !g.IsArchived));

                if (!string.IsNullOrEmpty(bestemming))
                {
                    geboekteGroepsreizen = geboekteGroepsreizen.Where(g => g.Bestemming.BestemmingsNaam == bestemming);
                    toekomstigeGroepsreizen = toekomstigeGroepsreizen.Where(g => g.Bestemming.BestemmingsNaam == bestemming);
                }

                geboekteGroepsreizen = geboekteGroepsreizen.OrderBy(g => g.Begindatum);
                toekomstigeGroepsreizen = toekomstigeGroepsreizen.OrderBy(g => g.Begindatum);

                var alleBestemmingen = await _unitOfWork.BestemmingRepository.GetAllAsync();

                return new GroepsreisViewModel
                {
                    GeboekteGroepsReizen = geboekteGroepsreizen.ToList(),
                    ToekomstigeGroepsReizen = toekomstigeGroepsreizen.ToList(),
                    AlleBestemmingen = alleBestemmingen.ToList()
                };
            }
        }
        #endregion
    }
}