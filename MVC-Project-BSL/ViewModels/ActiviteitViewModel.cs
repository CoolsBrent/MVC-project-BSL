using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace MVC_Project_BSL.ViewModels
{
    /// <summary>
    /// Een viewmodel voor de activiteit.
    /// </summary>
    public class ActiviteitViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Naam is verplicht.")]
        [StringLength(50, ErrorMessage = "Naam mag niet langer zijn dan 50 karakters.")]
        public string Naam { get; set; }

        [Required(ErrorMessage = "Beschrijving is verplicht.")]
        [StringLength(500, ErrorMessage = "Beschrijving mag niet langer zijn dan 500 karakters.")]
        public string Beschrijving { get; set; }

        // Hier worden de geselecteerde groepsreizen opgeslagen
        public List<int> SelectedGroepsreisIds { get; set; } = new List<int>();


        // Lijst van de beschikbare groepsreizen
        public IEnumerable<SelectListItem> GroepsreisOptions { get; set; } = new List<SelectListItem>();

        // Lijst van de programma's
        public List<ProgrammaViewModel> Programmas { get; set; } = new List<ProgrammaViewModel>();
    }
}
