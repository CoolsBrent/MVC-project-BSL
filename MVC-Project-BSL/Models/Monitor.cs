using MVC_Project_BSL.Models;
namespace MVC_Project_BSL.Models
{
	public class Monitor
	{
		public int Id { get; set; } // Unieke identifier voor de monitor
		public int PersoonId { get; set; } // Verwijzing naar de persoon
		public bool IsHoofdMonitor { get; set; } // Geeft aan of de monitor hoofdmonitor is

		public CustomUser Persoon { get; set; } // Navigatie-eigenschap naar de gebruiker
		public ICollection<GroepsreisMonitor> Groepsreizen { get; set; } = new List<GroepsreisMonitor>(); // Collectie van groepsreizen
	}
}