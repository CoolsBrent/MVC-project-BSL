using System.ComponentModel.DataAnnotations;

namespace MVC_Project_BSL.Models
{
    public class Groepsreis
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Begindatum is verplicht.")]
        public DateTime Begindatum { get; set; }

        [Required(ErrorMessage = "Einddatum is verplicht.")]
        public DateTime Einddatum { get; set; }

        [Required(ErrorMessage = "Prijs is verplicht.")]
        [Range(0, float.MaxValue, ErrorMessage = "Prijs moet een positief getal zijn.")]
        public float Prijs { get; set; }

        [Required(ErrorMessage = "Bestemming is verplicht.")]
        public int BestemmingId { get; set; }
        public Bestemming Bestemming { get; set; }


        // Zorg ervoor dat deze collecties worden geïnitialiseerd om lege lijsten te vermijden
        public ICollection<Kind> Kinderen { get; set; } = new List<Kind>();
        public ICollection<Monitor> Monitoren { get; set; } = new List<Monitor>();
        public ICollection<Onkosten> Onkosten { get; set; } = new List<Onkosten>();
        public ICollection<Activiteit> Activiteiten { get; set; } = new List<Activiteit>();
    }


}
