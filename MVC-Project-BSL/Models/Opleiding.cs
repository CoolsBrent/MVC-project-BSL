namespace MVC_Project_BSL.Models
{
	public class Opleiding
	{
		public int Id { get; set; }
		public string Naam { get; set; }
		public string Beschrijving { get; set; }
		public DateTime Begindatum { get; set; }
		public DateTime Einddatum { get; set; }
		public int AantalPlaatsen { get; set; }
		public int? OpleidingVereist { get; set; }

		public ICollection<OpleidingPersoon> OpleidingPersonen { get; set; }
	}

}