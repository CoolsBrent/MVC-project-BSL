using MVC_Project_BSL.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

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
    public DateTime Datum { get; set; }

    public string? Foto { get; set; }

    [NotMapped]
    [ValidateNever]
    public IFormFile? FotoFile { get; set; }

    public int GroepsreisId { get; set; }

    [ValidateNever]
    public Groepsreis Groepsreis { get; set; }
}
