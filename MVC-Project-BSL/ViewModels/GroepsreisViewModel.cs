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
        public string? BestemmingNaam { get; set; }
        [Required]
        public string? Bestemming { get; set; }
        [Required]
        public int MinLeeftijd { get; set; }
        [Required]
        public int MaxLeeftijd { get; set; }


        public string FotoUrl { get; set; }

        [Required]
        public string? Beschrijving { get; set; }

        [Required]
        public float Prijs { get; set; }

        public List<Activiteit> Activiteiten { get; set; } = new List<Activiteit>();
        public List<Kind> Kinderen { get; set; } = new List<Kind>();

        public List<Models.Monitor> Monitoren { get; set; } = new List<Models.Monitor>(); 
    }
}


