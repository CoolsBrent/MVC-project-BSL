namespace MVC_Project_BSL.Models
{
    public class Monitor
    {
        public int Id { get; set; }
        public bool IsHoofdMonitor { get; set; }

        public int PersoonId { get; set; }
        public CustomUser Persoon { get; set; }

        public ICollection<Groepsreis> Groepsreizen { get; set; }
    }

}
