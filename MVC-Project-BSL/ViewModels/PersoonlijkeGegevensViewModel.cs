using System.ComponentModel.DataAnnotations;

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
        [Required(ErrorMessage = "Naam is verplicht.")]
        public string Naam { get; set; }

        [Required(ErrorMessage = "Voornaam is verplicht.")]
        public string Voornaam { get; set; }

        [Required(ErrorMessage = "Geboortedatum is verplicht.")]
        public DateTime Geboortedatum { get; set; }
        [Required(ErrorMessage ="Allergieën verplicht")]
        public string Allergieën { get; set; }
        [Required(ErrorMessage = "Medicatie verplicht")]
        public string Medicatie { get; set; }
        public int PersoonId { get; set; }
    }
}
