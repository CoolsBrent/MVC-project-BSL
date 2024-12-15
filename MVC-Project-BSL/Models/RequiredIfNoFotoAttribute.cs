using System;
using System.ComponentModel.DataAnnotations;

namespace MVC_Project_BSL.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class RequiredIfNoFotoAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
        {
            var onkost = (Models.Onkosten)validationContext.ObjectInstance;

            // Controleer of er een bestaande foto is
            if (string.IsNullOrEmpty(onkost.Foto))
            {
                // Als er geen bestaande foto is, dan moet FotoFile verplicht zijn
                if (value == null)
                {
                    return new ValidationResult(ErrorMessage ?? "Het uploaden van een foto is verplicht.");
                }
            }

            return ValidationResult.Success!;
        }
    }
}
