using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using MVC_Project_BSL.Models;

namespace MVC_Project_BSL.ViewModels
{
    public class BestemmingViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Code is verplicht.")]
        public string Code { get; set; }

        [Required(ErrorMessage = "Bestemmingsnaam is verplicht.")]
        public string BestemmingsNaam { get; set; }

        [Required(ErrorMessage = "Beschrijving is verplicht.")]
        public string Beschrijving { get; set; }

        [Required(ErrorMessage = "Minimale leeftijd is verplicht.")]
        [Range(0, 120, ErrorMessage = "Minimale leeftijd moet tussen 0 en 120 liggen.")]
        public int MinLeeftijd { get; set; }

        [Required(ErrorMessage = "Maximale leeftijd is verplicht.")]
        [Range(0, 120, ErrorMessage = "Maximale leeftijd moet tussen 0 en 120 liggen.")]
        public int MaxLeeftijd { get; set; }

        // Voor het uploaden van nieuwe foto's
        public List<IFormFile> FotoBestanden { get; set; } = new List<IFormFile>();

        // Voor het weergeven van bestaande foto's
        [BindNever]
        public List<Foto> BestaandeFotos { get; set; } = new List<Foto>();

        // Voor het bijhouden welke foto's verwijderd moeten worden
        public List<int> VerwijderFotosIds { get; set; } = new List<int>();
    }
}
