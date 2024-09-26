using Microsoft.AspNetCore.Identity;

namespace MVC_Project_BSL.Models
{
    public class CustomUser : IdentityUser
    {
        public string Id { get; set; } // Dit moet een string zijn als je Identity gebruikt
        public string Naam { get; set; }
        public string Voornaam { get; set; }
        public string Straat { get; set; }
        public string Huisnummer { get; set; }
        public string Gemeente { get; set; }
        public string Postcode { get; set; }
        public DateTime Geboortedatum { get; set; }
        public string Huisdokter { get; set; }
        public string ContractNummer { get; set; }
        public string Email { get; set; }
        public string TelefoonNummer { get; set; }
        public string RekeningNummer { get; set; }
        public bool IsActief { get; set; }

        public ICollection<Kind> Kinderen { get; set; }
        public ICollection<Monitor> Monitoren { get; set; }
        public ICollection<OpleidingPersoon> Opleidingen { get; set; }
    }

}
