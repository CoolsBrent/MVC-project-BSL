using MVC_Project_BSL.Models;

namespace MVC_Project_BSL.ViewModels
{
	public class MonitorViewModel
	{
		public int Id { get; set; }
		public int PersoonId { get; set; } // Voeg PersoonId toe als je dat nodig hebt
		public string Voornaam { get; set; }   // Voornaam van de monitor
		public string Naam { get; set; }        // Achternaam van de monitor
		public bool IsHoofdMonitor { get; set; } // Is het een hoofdmonitor?
		public DateTime Geboortedatum { get; set; }
		public bool IsActief { get; set; }
		public Models.Monitor Monitor { get; set; }
		public List<Models.Monitor> Monitoren { get; set; } // Lijst van monitoren
		public CustomUser Persoon { get; set; } // Navigatie-eigenschap naar de gebruiker
		public List<GroepsreisOverzichtViewModel> Groepsreizen { get; set; } = new List<GroepsreisOverzichtViewModel>();

		// Overzicht van opleidingen
		public List<OpleidingOverzichtViewModel> Opleidingen { get; set; } = new List<OpleidingOverzichtViewModel>();
		

		public class GroepsreisOverzichtViewModel
		{
			public int Id { get; set; }
			public DateTime Begindatum { get; set; }
			public DateTime Einddatum { get; set; }
			public string? Naam { get; set; }
		}

		public class OpleidingOverzichtViewModel
		{
			public string Titel { get; set; }
			public DateTime BehaaldOp { get; set; }
		}
		public class CreateMonitorViewModel
		{
			public int PersoonId { get; set; }  // De Id van de bestaande gebruiker
			public bool IsHoofdMonitor { get; set; }
		}
	}
}



