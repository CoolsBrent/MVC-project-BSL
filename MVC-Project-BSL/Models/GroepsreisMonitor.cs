namespace MVC_Project_BSL.Models
{
	public class GroepsreisMonitor
	{
		public int GroepsreisId { get; set; }
		public Groepsreis Groepsreis { get; set; }

		public int MonitorId { get; set; }
		public Monitor Monitor { get; set; }
	}

}
