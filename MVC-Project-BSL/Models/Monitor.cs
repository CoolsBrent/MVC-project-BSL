namespace MVC_Project_BSL.Models
{
    public class Monitor
    {
        public string Id { get; set; }
        public string PersoonId { get; set; }
        public int GroepsreisDetailId { get; set; }
        public bool IsHoofdMonitor { get; set; }

        public CustomUser Persoon { get; set; }
        public Groepsreis GroepsreisDetail { get; set; }
    }

}
