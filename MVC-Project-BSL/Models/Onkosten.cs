using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using MVC_Project_BSL.Attributes;
using MVC_Project_BSL.Models;
namespace MVC_Project_BSL.Models
{
    public class Onkosten
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Titel is verplicht.")]
        public string Titel { get; set; }

        [Required(ErrorMessage = "Omschrijving is verplicht.")]
        public string Omschrijving { get; set; }

        [Required(ErrorMessage = "Bedrag is verplicht.")]
        public float Bedrag { get; set; }

        [Required(ErrorMessage = "Datum is verplicht.")]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]

        public DateTime Datum { get; set; }

        public string? Foto { get; set; }

        [NotMapped]
        [RequiredIfRoleHoofdmonitor(ErrorMessage = "Het uploaden van een foto is verplicht voor hoofdmonitoren.")]
        public IFormFile? FotoFile { get; set; }

        public int GroepsreisId { get; set; }

        [ValidateNever]
        public Groepsreis Groepsreis { get; set; }
        
        public string TypeOnkost { get; set; } // Dit veld geeft aan of het de hoofdmonitor of de verantwoordelijke betreft
        [NotMapped]
        public List<Onkosten> HoofdmonitorOnkosten = new List<Onkosten>();

        [NotMapped]
        public List<Onkosten> VerantwoordelijkeOnkosten = new List<Onkosten>();
        [NotMapped]
        public List<Onkosten> AlleOnkosten = new List<Onkosten>();
    }
}
