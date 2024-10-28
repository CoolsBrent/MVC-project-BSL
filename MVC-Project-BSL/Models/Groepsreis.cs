using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MVC_Project_BSL.Models
{
	public class Groepsreis
	{
		public int Id { get; set; }

		[Required(ErrorMessage = "Begindatum is verplicht.")]
		[DataType(DataType.Date)]
		public DateTime Begindatum { get; set; }

		[Required(ErrorMessage = "Einddatum is verplicht.")]
		[DataType(DataType.Date)]
		public DateTime Einddatum { get; set; }

		[Required(ErrorMessage = "Prijs is verplicht.")]
		[Range(0, float.MaxValue, ErrorMessage = "Prijs moet een positief getal zijn.")]
		public float Prijs { get; set; }

		[Required(ErrorMessage = "Bestemming is verplicht.")]
		public int BestemmingId { get; set; }
		public Bestemming? Bestemming { get; set; }

		public ICollection<GroepsreisMonitor>? Monitoren { get; set; }
		public ICollection<Programma>? Programmas { get; set; }
		public ICollection<Onkosten>? Onkosten { get; set; }
		public ICollection<Deelnemer>? Deelnemers { get; set; }
		[NotMapped]
		public ICollection<Monitor> BeschikbareMonitoren { get; set; } = new List<Monitor>();
		[NotMapped]
		public ICollection<Kind> BeschikbareDeelnemers { get; set; } = new List<Kind>();

	}


}