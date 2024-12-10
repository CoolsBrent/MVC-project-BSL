using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace MVC_Project_BSL.Models
{
	public class CustomUser : IdentityUser<int>
	{
		[Required(ErrorMessage = "Naam is verplicht.")]
		public string Naam { get; set; }
		[Required(ErrorMessage = "Voornaam is verplicht.")]
		public string Voornaam { get; set; }
		[Required(ErrorMessage = "Straat is verplicht.")]
		public string Straat { get; set; }
		[Required(ErrorMessage = "Huisnummer is verplicht.")]
		public string Huisnummer { get; set; }
		[Required(ErrorMessage = "Gemeente is verplicht.")]
		public string Gemeente { get; set; }
		public string Postcode { get; set; }
		[Required(ErrorMessage = "Geboortedatum is verplicht.")]
		public DateTime Geboortedatum { get; set; }
		[Required(ErrorMessage = "Huisdokter is verplicht.")]
		public string Huisdokter { get; set; }
		public string? ContractNummer { get; set; }
		[Required(ErrorMessage = "Naam is verplicht.")]
		public string TelefoonNummer { get; set; }
		[Required(ErrorMessage = "Rekeningnummer is verplicht.")]
		public string RekeningNummer { get; set; }
		public bool IsActief { get; set; }

		public ICollection<Kind> Kinderen { get; set; }
		public ICollection<Monitor> Monitoren { get; set; }
		public ICollection<OpleidingPersoon> Opleidingen { get; set; }
	}

}