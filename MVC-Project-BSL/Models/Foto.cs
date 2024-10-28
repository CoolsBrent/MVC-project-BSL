namespace MVC_Project_BSL.Models
{
	public class Foto
	{
		public int Id { get; set; }
		public string Naam { get; set; }

		public int BestemmingId { get; set; }
		public Bestemming Bestemming { get; set; }
	}

}