using Bookkeeping.Data.Models;
using Bookkeeping.Data.Repository.Interfaces;

namespace Bookkeeping.WebUi.Models
{
    public class TaskFilter: ITaskFilter
    {
        public int Offset { get; set; }
        public int Limit { get; set; }
        public TaskType Type { get; set; }
        public string Inn { get; set; }
        public string Name { get; set; }
        public string Purpose { get; set; }
        public int[] AgentIds { get; set; }
        public bool IsArchive { get; set; }
    }
}