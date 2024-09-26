namespace MVC_Project_BSL.Models
{
    public class OpleidingPersoon
    {
        public string Id { get; set; }

        public int OpleidingId { get; set; }
        public Opleiding Opleiding { get; set; }

        public string PersoonId { get; set; }
        public CustomUser Persoon { get; set; }
    }

}
