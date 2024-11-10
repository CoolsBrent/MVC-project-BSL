using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MVC_Project_BSL.Data.UnitOfWork;
using MVC_Project_BSL.Models;

namespace MVC_Project_BSL.Services
{
    public class MonitorService
    {
        #region Private Fields
        private readonly UserManager<CustomUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        #endregion

        #region Constructor
        /// <summary>
        /// Initialiseert een nieuwe instantie van de MonitorService met gebruikersbeheer en unit of work.
        /// </summary>
        /// <param name="userManager">De UserManager voor gebruikersbeheer.</param>
        /// <param name="unitOfWork">De UnitOfWork voor toegang tot repository's.</param>
        public MonitorService(UserManager<CustomUser> userManager, IUnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _unitOfWork = unitOfWork;
        }
        #endregion

        #region Public Methods

        /// <summary>
        /// Wijzigt de opgegeven monitor naar hoofdmonitor binnen de groepsreis en past de gebruikerstoegang aan.
        /// </summary>
        /// <param name="groepsreisId">De ID van de groepsreis.</param>
        /// <param name="monitorId">De ID van de monitor.</param>
        /// <returns>Een IActionResult dat de wijziging bevestigt.</returns>
        public async Task<IActionResult> MaakHoofdmonitor(int groepsreisId, int monitorId)
        {
            var groepsreis = await _unitOfWork.GroepsreisRepository.GetByIdWithIncludesAsync(groepsreisId, g => g.Monitoren);

            if (groepsreis == null)
            {
                return new NotFoundObjectResult("Groepsreis niet gevonden");
            }

            foreach (var gm in groepsreis.Monitoren)
            {
                if (gm.Monitor != null)
                {
                    gm.Monitor.IsHoofdMonitor = false;
                }
            }

            var geselecteerdeMonitor = await _unitOfWork.MonitorRepository.GetByIdAsync(monitorId);
            if (geselecteerdeMonitor == null)
            {
                return new NotFoundObjectResult("Monitor niet gevonden");
            }

            geselecteerdeMonitor.IsHoofdMonitor = true;

            _unitOfWork.MonitorRepository.Update(geselecteerdeMonitor);
            _unitOfWork.SaveChanges();

            var user = await _userManager.FindByIdAsync(geselecteerdeMonitor.PersoonId.ToString());
            if (user != null)
            {
                if (await _userManager.IsInRoleAsync(user, "Monitor"))
                {
                    await _userManager.RemoveFromRoleAsync(user, "Monitor");
                }

                if (!await _userManager.IsInRoleAsync(user, "Hoofdmonitor"))
                {
                    await _userManager.AddToRoleAsync(user, "Hoofdmonitor");
                }
            }

            return new RedirectToActionResult("Detail", "Groepsreis", new { id = groepsreisId });
        }

        /// <summary>
        /// Wijzigt de opgegeven hoofdmonitor naar gewone monitor binnen de groepsreis en past de gebruikerstoegang aan.
        /// </summary>
        /// <param name="groepsreisId">De ID van de groepsreis.</param>
        /// <param name="monitorId">De ID van de monitor.</param>
        /// <returns>Een IActionResult dat de wijziging bevestigt.</returns>
        public async Task<IActionResult> MaakGewoneMonitor(int groepsreisId, int monitorId)
        {
            var groepsreis = await _unitOfWork.GroepsreisRepository.GetByIdWithIncludesAsync(groepsreisId, g => g.Monitoren);

            if (groepsreis == null)
            {
                return new NotFoundObjectResult("Groepsreis niet gevonden");
            }

            var geselecteerdeMonitor = await _unitOfWork.MonitorRepository.GetByIdAsync(monitorId);
            if (geselecteerdeMonitor == null)
            {
                return new NotFoundObjectResult("Monitor niet gevonden");
            }

            geselecteerdeMonitor.IsHoofdMonitor = false;
            _unitOfWork.MonitorRepository.Update(geselecteerdeMonitor);
            _unitOfWork.SaveChanges();

            var user = await _userManager.FindByIdAsync(geselecteerdeMonitor.PersoonId.ToString());
            if (user != null)
            {
                if (await _userManager.IsInRoleAsync(user, "Hoofdmonitor"))
                {
                    await _userManager.RemoveFromRoleAsync(user, "Hoofdmonitor");
                }

                if (!await _userManager.IsInRoleAsync(user, "Monitor"))
                {
                    await _userManager.AddToRoleAsync(user, "Monitor");
                }
            }

            return new RedirectToActionResult("Detail", "Groepsreis", new { id = groepsreisId });
        }
        #endregion
    }
}