using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bookkeeping.Data.Models
{
    [NotMapped]
    public class ManagerTask : Task
    {
        public IList<TaskDecision> Decisions { get; set; }
        public TaskType Status { get; set; }

        public string DecisionsJson => JsonConvert.SerializeObject(Decisions);

        public ManagerTask(){ }

        public ManagerTask(Task task)
        {
            ClientId = task.ClientId;
            Date = task.Date;
            Id = task.Id;
            Inn = task.Inn;
            IsArchive = task.IsArchive;
            Name = task.Name;
            PurposeOfPayment = task.PurposeOfPayment;
            TotalCount = task.TotalCount;
        }
    }
}