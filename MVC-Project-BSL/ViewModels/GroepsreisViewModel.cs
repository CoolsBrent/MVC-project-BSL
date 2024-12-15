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



        // Lijst van programma's die bij de groepsreis horen
        public List<ProgrammaViewModel> Programmas { get; set; } = new List<ProgrammaViewModel>();


        public IEnumerable<Groepsreis> ActieveGroepsreizen { get; set; }
        public IEnumerable<Groepsreis> GearchiveerdeGroepsreizen { get; set; }

        public List<Models.Monitor> Monitoren { get; set; } = new List<Models.Monitor>();

        // Filteropties die vanuit de view worden gebruikt
        public int? MinLeeftijdFilter { get; set; }
        public int? MaxLeeftijdFilter { get; set; }
        public DateTime? BegindatumFilter { get; set; }

        public List<Groepsreis> GeboekteGroepsReizen { get; set; } = new List<Groepsreis> { };
        public List<Groepsreis> ToekomstigeGroepsReizen { get; set; } = new List<Groepsreis> { };
        public List<Groepsreis> AlleGroepsReizen { get; set; } = new List<Groepsreis> { };
        public List<Bestemming> AlleBestemmingen { get; set; } = new List<Bestemming> { };
		public List<Onkosten> Onkosten { get; set; } = new List<Onkosten> { };
        public List<Opleiding> AlleOpleidingen { get; set; } = new List<Opleiding> { };
		public List<Opleiding> ToekomstigeOpleidingen { get; set; } = new List<Opleiding> { };
		public List<Opleiding> IngeschrevenOpleidingen { get; set; } = new List<Opleiding> { };
	}
}