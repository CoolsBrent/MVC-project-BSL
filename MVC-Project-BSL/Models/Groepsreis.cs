namespace MVC_Project_BSL.Models
{
    public class Groepsreis
    {
        public int Id { get; set; }
        public DateTime Begindatum { get; set; }
        public DateTime Einddatum { get; set; }
        public float Prijs { get; set; }

        public int BestemmingId { get; set; }
        public Bestemming Bestemming { get; set; }

        public ICollection<Monitor> Monitoren { get; set; }

        public ICollection<Activiteit> Activiteiten { get; set; }
        public ICollection<Kind> Kinderen { get; set; }
        public ICollection<Onkosten> Onkosten { get; set; }
    }


}
