namespace MVC_Project_BSL.Models
{
    public class Kind
    {
        public int Id { get; set; }
        public string Naam { get; set; }
        public string Voornaam { get; set; }
        public DateTime Geboortedatum { get; set; }
        public string Allergieen { get; set; }
        public string Medicatie { get; set; }

        public string PersoonId { get; set; }
        public CustomUser Persoon { get; set; }

        public ICollection<Groepsreis> Groepsreizen { get; set; }
    }

}
