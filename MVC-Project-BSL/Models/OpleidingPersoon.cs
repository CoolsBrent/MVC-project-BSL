using System;

namespace MVC_Project_BSL.Models
{
	public class OpleidingPersoon
	{
		public int Id { get; set; }
		public int OpleidingId { get; set; }
		public int PersoonId { get; set; }

		public Opleiding Opleiding { get; set; }
		public CustomUser Persoon { get; set; }


	}

}