namespace MVC_Project_BSL.Models
{
    public class Monitor
    {
        public string Id { get; set; }
        public bool IsHoofdMonitor { get; set; } = false;

        public string PersoonId { get; set; }
        public CustomUser Persoon { get; set; }

        public ICollection<Groepsreis> Groepsreizen { get; set; } = new List<Groepsreis>();
    }

}
