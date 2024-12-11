
using System.ComponentModel.DataAnnotations.Schema;

namespace MVC_Project_BSL.Models
{
    public class Deelnemer
    {
        public int Id { get; set; }
        public int KindId { get; set; }
        public int GroepsreisDetailId { get; set; }
		[Column(TypeName = "nvarchar(max)")]
		public string? Opmerkingen { get; set; }

		public int? ReviewScore { get; set; }

		[Column(TypeName = "nvarchar(max)")]
		public string? Review { get; set; }

        public Kind Kind { get; set; }
        public Groepsreis GroepsreisDetail { get; set; }
    }
}
