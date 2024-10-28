namespace MVC_Project_BSL.Models
{
	public class Onkosten
	{
		public int Id { get; set; }
		public int GroepsreisId { get; set; }
		public string Titel { get; set; }
		public string Omschrijving { get; set; }
		public float Bedrag { get; set; }
		public DateTime Datum { get; set; }
		public string Foto { get; set; }

		public Groepsreis Groepsreis { get; set; }
	}

}