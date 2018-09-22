namespace Bookkeeping.WebUi.Models
{
    public class AgentTaskFilterViewModel
    {
        public int Offset { get; set; }
        public int Limit { get; set; }
        public bool IsDone { get; set; }
        public string ResolutionError { get; set; }
    }
}