namespace MVC_Project_BSL.ViewModels
{
    public class PersoonlijkeGegevensViewModel
    {
        // Persoonlijke gegevens
        public int Id { get; set; }
        public string Naam { get; set; }
        public string Voornaam { get; set; }
        public DateTime Geboortedatum { get; set; }
        public string Huisdokter { get; set; }
        public string TelefoonNummer { get; set; }
        public string RekeningNummer { get; set; }
        public bool IsActief { get; set; }

        // Gegevens van de kinderen
        public List<KindGegevensViewModel> Kinderen { get; set; } = new List<KindGegevensViewModel>();

    }

    public class KindGegevensViewModel
    {
        public int Id { get; set; }
        public string Naam { get; set; }
        public string Voornaam { get; set; }
        public DateTime Geboortedatum { get; set; }
        public string Allergieën { get; set; }
        public string Medicatie { get; set; }
        public int PersoonId { get; set; }
    }
}
