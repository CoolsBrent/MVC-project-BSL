using System.ComponentModel.DataAnnotations;

namespace MVC_Project_BSL.Models
{
	public class Kind
	{
		public int Id { get; set; } // Primary Key
		public int PersoonId { get; set; } // Foreign Key naar Persoon (via CustomUser)

		[Required(ErrorMessage = "Naam is verplicht.")]
		public string Naam { get; set; }

		[Required(ErrorMessage = "Voornaam is verplicht.")]
		public string Voornaam { get; set; }

    [Required(ErrorMessage = "Geboortedatum is verplicht.")]
		public DateTime Geboortedatum { get; set; }
    public string? Allergieen { get; set; } = "Geen";
    public string? Medicatie { get; set; } = "Geen";

		// Navigatie-eigenschappen
		public CustomUser Persoon { get; set; }
	}
}