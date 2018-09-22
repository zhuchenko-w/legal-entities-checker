using Bookkeeping.Data.Models;
using System.Collections.Generic;

namespace Bookkeeping.WebUi.Models
{
    public class ManagerTasksViewModel
    {
        public bool IsArchived { get; set; }
        public string TasksError { get; set; }
        public IList<ManagerTask> Tasks { get; set; }
        public IList<Agent> Agents { get; set; }
        public int PageSize { get; set; }
    }
}