using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MVC_Project_BSL.Models
{
	public class Bestemming
	{
		public int Id { get; set; }

		[Required(ErrorMessage = "Code is verplicht.")]
		public string Code { get; set; }

		[Required(ErrorMessage = "Bestemmingsnaam is verplicht.")]
		public string BestemmingsNaam { get; set; }

		[Required(ErrorMessage = "Beschrijving is verplicht.")]
		public string Beschrijving { get; set; }

		[Required(ErrorMessage = "Minimale leeftijd is verplicht.")]
		[Range(0, 120, ErrorMessage = "Minimale leeftijd moet tussen 0 en 120 liggen.")]
		public int MinLeeftijd { get; set; }

		[Required(ErrorMessage = "Maximale leeftijd is verplicht.")]
		[Range(0, 120, ErrorMessage = "Maximale leeftijd moet tussen 0 en 120 liggen.")]
		public int MaxLeeftijd { get; set; }

		public ICollection<Foto> Fotos { get; set; }
		public ICollection<Groepsreis> Groepsreizen { get; set; }
	}
}