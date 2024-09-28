using MVC_Project_BSL.Models;
using System.ComponentModel.DataAnnotations;

namespace MVC_Project_BSL.ViewModels
{
    public class GroepsreisViewModel
    {
        public int Id { get; set; }

        [Required]
        public DateTime Begindatum { get; set; }

        [Required]
        public DateTime Einddatum { get; set; }

        [Required]
        public string? Bestemming { get; set; }

        [Required]
        public List<string> FotoUrls { get; set; } = new List<string>(); // Voor eenvoudige tekstinvoer

        [Required]
        public string? Beschrijving { get; set; }

        [Required]
        public string? Leeftijdscategorie { get; set; }

        [Required]
        public decimal Prijs { get; set; }

        public List<Activiteit> Activiteiten { get; set; } = new List<Activiteit>();
    }
}

