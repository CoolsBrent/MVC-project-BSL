using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using MVC_Project_BSL.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


namespace MVC_Project_BSL.Models
{
    public class Opleiding
    {
        public Opleiding() 
        {
            OpleidingPersonen = new List<OpleidingPersoon>();
        }
        public int Id { get; set; }

        [Required(ErrorMessage = "Naam is verplicht.")]
        [StringLength(100, ErrorMessage = "Naam mag maximaal 100 tekens lang zijn.")]
        public string Naam { get; set; }

        [Required(ErrorMessage = "Beschrijving is verplicht.")]
        public string Beschrijving { get; set; }

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

        public int? OpleidingVereist { get; set; }

        [ValidateNever]
        public ICollection<OpleidingPersoon> OpleidingPersonen { get; set; }




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

            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                var currentValue = (DateTime)value;

                var property = validationContext.ObjectType.GetProperty(_comparisonProperty);
                if (property == null)
                    throw new ArgumentException("Property with this name not found");

                var comparisonValue = (DateTime)property.GetValue(validationContext.ObjectInstance);

                if (currentValue < comparisonValue)
                    return new ValidationResult(ErrorMessage);

                return ValidationResult.Success;
            }
        }
    }
}
