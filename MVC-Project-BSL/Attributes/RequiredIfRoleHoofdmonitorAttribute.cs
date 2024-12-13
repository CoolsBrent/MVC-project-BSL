using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace MVC_Project_BSL.Attributes
{
    public class RequiredIfRoleHoofdmonitorAttribute : ValidationAttribute, IClientModelValidator
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            // Haal de waarde van TypeOnkost op
            var onkosten = (MVC_Project_BSL.Models.Onkosten)validationContext.ObjectInstance;
            var userRole = onkosten.TypeOnkost; // Verwacht "Hoofdmonitor" of "Verantwoordelijke"

            // Controleer of de gebruiker een hoofdmonitor is en of er een bestand is geüpload
            if (userRole == "Hoofdmonitor" && value == null)
            {
                return new ValidationResult(ErrorMessage ?? "Het uploaden van een foto is verplicht voor hoofdmonitoren.");
            }

            // Validatie slaagt als de gebruiker geen hoofdmonitor is of een bestand is geüpload
            return ValidationResult.Success;
        }

        // Client-side validatie (optioneel, voor JavaScript)
        public void AddValidation(ClientModelValidationContext context)
        {
            context.Attributes.Add("data-val", "true");
            context.Attributes.Add("data-val-requiredifrolehoofdmonitor", ErrorMessage ?? "Het uploaden van een foto is verplicht voor hoofdmonitoren.");
        }
    }
}
