using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MVC_Project_BSL.Models
{
    public class Opleiding
    {
        public Opleiding()
        {
            OpleidingPersonen = new List<OpleidingPersoon>();
            OpleidingenAfhankelijk = new List<Opleiding>();
        }

        public int Id { get; set; }

        [Required(ErrorMessage = "Naam is verplicht.")]
        [StringLength(100, ErrorMessage = "Naam mag maximaal 100 tekens lang zijn.")]
        public required string Naam { get; set; }

        [Required(ErrorMessage = "Beschrijving is verplicht.")]
        public required string Beschrijving { get; set; }

        [Required(ErrorMessage = "Begindatum is verplicht.")]
        [DataType(DataType.Date)]
        public DateTime Begindatum { get; set; }

        [Required(ErrorMessage = "Einddatum is verplicht.")]
        [DataType(DataType.Date)]
        [DateGreaterThan("Begindatum", ErrorMessage = "Einddatum moet na de begindatum liggen.")]
        public DateTime Einddatum { get; set; }

        [Required(ErrorMessage = "Aantal plaatsen is verplicht.")]
        [Range(1, int.MaxValue, ErrorMessage = "Aantal plaatsen moet minimaal 1 zijn.")]
        public int AantalPlaatsen { get; set; }

        // Foreign key property voor de vereiste opleiding
        public int? OpleidingVereistId { get; set; }

        // Navigatie-eigenschap voor de vereiste opleiding
        [ForeignKey("OpleidingVereistId")]
        public Opleiding? OpleidingVereist { get; set; }

        // Navigatie-eigenschap voor afhankelijke opleidingen
        [InverseProperty("OpleidingVereist")]
        public ICollection<Opleiding> OpleidingenAfhankelijk { get; set; }

        [ValidateNever]
        public ICollection<OpleidingPersoon> OpleidingPersonen { get; set; }
		[NotMapped]
		public ICollection<CustomUser>? Monitoren { get; set; }

		[NotMapped]
		public ICollection<Monitor> BeschikbareMonitoren { get; set; } = new List<Monitor>();
		[NotMapped]
		public int IngeschrevenPersonen { get; set; }
		[NotMapped]
		public int AantalBeschikbarePlaatsen
		{
			get
			{
				// Het aantal beschikbare plaatsen is het verschil tussen AantalPlaatsen en IngeschrevenPersonen
				return AantalPlaatsen - IngeschrevenPersonen;
			}
		}
		[NotMapped]
		public bool IsIngeschreven { get; set; }

		/// <summary>
		/// Om te checken dat de datum van de start eerder is dan de einddatum
		/// </summary>
		public class DateGreaterThanAttribute : ValidationAttribute
        {
            private readonly string _comparisonProperty;

            public DateGreaterThanAttribute(string comparisonProperty)
            {
                _comparisonProperty = comparisonProperty;
            }

            protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
            {
				if (value == null)
				{
					// Indien nodig, een foutmelding retourneren als de waarde null is
					return new ValidationResult("Een waarde is vereist.");
				}
				var currentValue = (DateTime)value;

                var property = validationContext.ObjectType.GetProperty(_comparisonProperty);
                if (property == null)
                    throw new ArgumentException("Property with this name not found");

                var comparisonValue = (DateTime)property.GetValue(validationContext.ObjectInstance)!;

                if (currentValue < comparisonValue)
                    return new ValidationResult(ErrorMessage);

                return ValidationResult.Success!;
            }
        }
    }
}
