namespace MVC_Project_BSL.Models
{
    public class Bestemming
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string BestemmingsNaam { get; set; }
        public string Beschrijving { get; set; }
        public int MinLeeftijd { get; set; }
        public int MaxLeeftijd { get; set; }

        public ICollection<Foto> Fotos { get; set; }
        public ICollection<Groepsreis> Groepsreizen { get; set; }
    }

}
