using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using MVC_Project_BSL.Attributes;
using MVC_Project_BSL.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MVC_Project_BSL.ViewModels
{
    public class OnkostenViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Titel is verplicht.")]
        public string Titel { get; set; }

        [Required(ErrorMessage = "Omschrijving is verplicht.")]
        public string Omschrijving { get; set; }

        [Required(ErrorMessage = "Bedrag is verplicht.")]
        public float Bedrag { get; set; }

        [Required(ErrorMessage = "Datum is verplicht.")]
        public DateTime Datum { get; set; }

        public string? Foto { get; set; }

        [NotMapped]
        [RequiredIfNoFoto(ErrorMessage = "Het uploaden van een foto is verplicht.")]
        public IFormFile? FotoFile { get; set; }

        public int GroepsreisId { get; set; }

        [ValidateNever]
        public Groepsreis Groepsreis { get; set; }
    }
}
