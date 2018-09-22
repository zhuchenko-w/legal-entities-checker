using System.ComponentModel.DataAnnotations.Schema;

namespace Bookkeeping.Data.Models
{
    [NotMapped]
    public class AgentTask : Task
    {
        public bool? Decision { get; set; }

        public AgentTask() { }

        public AgentTask(Task task)
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