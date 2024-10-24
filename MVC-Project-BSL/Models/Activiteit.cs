namespace MVC_Project_BSL.Models
{
	public class Activiteit
	{
		public int Id { get; set; }
		public string Naam { get; set; }
		public string Beschrijving { get; set; }

		public ICollection<Programma> Programmas { get; set; }
	}

}
