using System.ComponentModel.DataAnnotations;

namespace MVC_Project_BSL.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Email is verplicht.")]
        [EmailAddress(ErrorMessage = "Voer een geldig emailadres in.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Naam is verplicht.")]
        public string Naam { get; set; }

        [Required(ErrorMessage = "Voornaam is verplicht.")]
        public string Voornaam { get; set; }

        [Required(ErrorMessage = "Wachtwoord is verplicht.")]
        [StringLength(100, ErrorMessage = "Het {0} moet minstens {2} en maximaal {1} tekens bevatten.", MinimumLength = 6)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9]).{8,}$",
            ErrorMessage = "Het wachtwoord moet minstens 8 tekens bevatten, met een hoofdletter, een kleine letter, een cijfer en een speciaal teken.")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Bevestig het wachtwoord.")]
        [Compare("Password", ErrorMessage = "De wachtwoorden komen niet overeen.")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Geboortedatum is verplicht.")]
        [DataType(DataType.Date)]
        public DateTime Geboortedatum { get; set; }

        [Required(ErrorMessage = "Telefoonnummer is verplicht.")]
        public string TelefoonNummer { get; set; }

        [Required(ErrorMessage = "Rekeningnummer is verplicht.")]
        public string RekeningNummer { get; set; }

        [Required(ErrorMessage = "Huisdokter is verplicht.")]
        public string Huisdokter { get; set; }

        [Required(ErrorMessage = "Straat is verplicht.")]
        public string Straat { get; set; }

        [Required(ErrorMessage = "Huisnummer is verplicht.")]
        public string Huisnummer { get; set; }

        [Required(ErrorMessage = "Gemeente is verplicht.")]
        public string Gemeente { get; set; }

        [Required(ErrorMessage = "Postcode is verplicht.")]
        public string Postcode { get; set; }
    }
}
