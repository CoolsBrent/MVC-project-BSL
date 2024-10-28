using MVC_Project_BSL.Models;
using System.ComponentModel.DataAnnotations;

namespace MVC_Project_BSL.ViewModels
{
    public class GroepsreisViewModel
    {
        public int Id { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime Begindatum { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime Einddatum { get; set; }

        [Required]
        public string? Bestemming { get; set; }

        [Required]
        public List<string> FotoUrls { get; set; } = new List<string>(); // Voor eenvoudige tekstinvoer

        [Required]
        public string? Beschrijving { get; set; }

        [Required]
        public decimal Prijs { get; set; }

        // Verander Leeftijdscategorie naar MinLeeftijd en MaxLeeftijd voor filtering
        [Required]
        public int MinLeeftijd { get; set; }

        [Required]
        public int MaxLeeftijd { get; set; }



        public List<Activiteit> Activiteiten { get; set; } = new List<Activiteit>();


        public List<Monitor> Monitoren { get; set; } = new List<Monitor>();
        // Filteropties die vanuit de view worden gebruikt
        public int? MinLeeftijdFilter { get; set; }
        public int? MaxLeeftijdFilter { get; set; }
        public DateTime? BegindatumFilter { get; set; }

        public List<Groepsreis> GeboekteGroepsReizen { get; set; } = new List<Groepsreis> { };
        public List<Groepsreis> ToekomstigeGroepsReizen { get; set; } = new List<Groepsreis> { };
        public List<Groepsreis> AlleGroepsReizen { get; set; } = new List<Groepsreis> { };
        public List<Bestemming> AlleBestemmingen { get; set; } = new List<Bestemming> { };


    }
}