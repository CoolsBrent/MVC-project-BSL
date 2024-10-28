using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MVC_Project_BSL.Data.UnitOfWork;
using MVC_Project_BSL.Models;

public class MonitorService
{
	private readonly UserManager<CustomUser> _userManager;
	private readonly IUnitOfWork _unitOfWork;

	public MonitorService(UserManager<CustomUser> userManager, IUnitOfWork unitOfWork)
	{
		_userManager = userManager;
		_unitOfWork = unitOfWork;
	}

	public async Task<IActionResult> MaakHoofdmonitor(int groepsreisId, int monitorId)
	{
		var groepsreis = await _unitOfWork.GroepsreisRepository.GetByIdWithIncludesAsync(groepsreisId, g => g.Monitoren);

		if (groepsreis == null)
		{
			return new NotFoundObjectResult("Groepsreis niet gevonden");
		}

		// Zet alle monitoren terug naar niet-hoofdmonitor
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

		// Zet de geselecteerde monitor als hoofdmonitor
		geselecteerdeMonitor.IsHoofdMonitor = true;

		// Opslaan van wijzigingen
		_unitOfWork.MonitorRepository.Update(geselecteerdeMonitor);
		_unitOfWork.SaveChanges();

		// Update de rol van de gebruiker
		var user = await _userManager.FindByIdAsync(geselecteerdeMonitor.PersoonId.ToString());
		if (user != null)
		{
			// Verwijder de rol "Monitor" als deze al is toegewezen
			if (await _userManager.IsInRoleAsync(user, "Monitor"))
			{
				await _userManager.RemoveFromRoleAsync(user, "Monitor");
			}

			// Voeg de rol "Hoofdmonitor" toe
			if (!await _userManager.IsInRoleAsync(user, "Hoofdmonitor"))
			{
				await _userManager.AddToRoleAsync(user, "Hoofdmonitor");
			}
		}

		return new RedirectToActionResult("Detail", "Groepsreis", new { id = groepsreisId });
	}

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

		// Update de monitor-status naar "niet hoofdmonitor"
		geselecteerdeMonitor.IsHoofdMonitor = false;
		_unitOfWork.MonitorRepository.Update(geselecteerdeMonitor);
		_unitOfWork.SaveChanges();

		// Update de rol van de gebruiker
		var user = await _userManager.FindByIdAsync(geselecteerdeMonitor.PersoonId.ToString());
		if (user != null)
		{
			// Verwijder de rol "Hoofdmonitor" als deze al is toegewezen
			if (await _userManager.IsInRoleAsync(user, "Hoofdmonitor"))
			{
				await _userManager.RemoveFromRoleAsync(user, "Hoofdmonitor");
			}

			// Voeg de rol "Monitor" toe als de gebruiker deze nog niet heeft
			if (!await _userManager.IsInRoleAsync(user, "Monitor"))
			{
				await _userManager.AddToRoleAsync(user, "Monitor");
			}
		}

		return new RedirectToActionResult("Detail", "Groepsreis", new { id = groepsreisId });
	}
}
