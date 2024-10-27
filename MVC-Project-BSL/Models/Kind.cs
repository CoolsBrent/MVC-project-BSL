namespace MVC_Project_BSL.Models
{
	public class Kind
	{
		public int Id { get; set; } // Primary Key
		public int PersoonId { get; set; } // Foreign Key naar Persoon (via CustomUser)

		public string Naam { get; set; }
		public string Voornaam { get; set; }
		public DateTime Geboortedatum { get; set; }
		public string Allergieën { get; set; }
		public string Medicatie { get; set; }

		// Navigatie-eigenschappen
		public CustomUser Persoon { get; set; }
	}

}