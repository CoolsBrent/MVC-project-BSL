namespace MVC_Project_BSL.Models
{
    public class Programma
    {
        public int Id { get; set; }
        public int ActiviteitId { get; set; }
        public int GroepsreisId { get; set; }

        public Activiteit Activiteit { get; set; }
        public Groepsreis Groepsreis { get; set; }
    }
}
