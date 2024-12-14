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
		public bool IsArchived { get; set; } = false; // Standaard actief
		public ICollection<GroepsreisMonitor>? Monitoren { get; set; }
		public ICollection<Programma>? Programmas { get; set; }
		public List<Onkosten>? Onkosten { get; set; } = new List<Onkosten>();
		public ICollection<Deelnemer>? Deelnemers { get; set; }
        public int MaxAantalDeelnemers { get; set; }
        public ICollection<Deelnemer> Wachtlijst { get; set; } = new List<Deelnemer>();
        [NotMapped]
		public ICollection<Monitor> BeschikbareMonitoren { get; set; } = new List<Monitor>();
		[NotMapped]
		public ICollection<GroepsreisMonitor> IngeschrevenMonitoren { get; set; } = new List<GroepsreisMonitor>();
		[NotMapped]
		public ICollection<Kind> BeschikbareDeelnemers { get; set; } = new List<Kind>();

	}


}