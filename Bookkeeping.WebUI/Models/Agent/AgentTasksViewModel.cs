using Bookkeeping.Data.Models;
using System.Collections.Generic;

namespace Bookkeeping.WebUi.Models
{
    public class AgentTasksViewModel
    {
        public int DoneCount { get; set; }
        public int InWorkCount { get; set; }
        public bool IsDone { get; set; }
        public string ResolutionError { get; set; }
        public string TasksError { get; set; }
        public IList<AgentTask> Tasks { get; set; }
        public int PageSize { get; set; }
    }
}