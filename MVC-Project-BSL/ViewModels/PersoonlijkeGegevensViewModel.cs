using System.ComponentModel.DataAnnotations;

namespace MVC_Project_BSL.ViewModels
{
    public class PersoonlijkeGegevensViewModel
    {
		// Persoonlijke gegevens
		public int Id { get; set; }
		[Required(ErrorMessage = "Naam is verplicht.")]

		public string Naam { get; set; }
		[Required(ErrorMessage = "Voornaam is verplicht.")]

		public string Voornaam { get; set; }
		[Required(ErrorMessage = "Geboortedatum is verplicht.")]

		public DateTime Geboortedatum { get; set; }
		[Required(ErrorMessage = "Huisdokter is verplicht.")]
		public string Huisdokter { get; set; }
		[Required(ErrorMessage = "Telefoonnummer is verplicht.")]
		public string TelefoonNummer { get; set; }
		[Required(ErrorMessage = "Rekeningnummer is verplicht.")]
		public string RekeningNummer { get; set; }
		public bool IsActief { get; set; }

		// Gegevens van de kinderen
		public List<KindGegevensViewModel> Kinderen { get; set; } = new List<KindGegevensViewModel>();

	}

	public class KindGegevensViewModel
	{
		public int Id { get; set; }
		[Required(ErrorMessage = "Naam is verplicht.")]
		public string Naam { get; set; }

		[Required(ErrorMessage = "Voornaam is verplicht.")]
		public string Voornaam { get; set; }

		[Required(ErrorMessage = "Geboortedatum is verplicht.")]
		public DateTime Geboortedatum { get; set; }
		public string? Allergieen { get; set; } = "Geen";
		public string? Medicatie { get; set; } = "Geen";
		public int PersoonId { get; set; }
	}
}
