using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bookkeeping.Data.Models
{
    [NotMapped]
    public class TaskDecision
    {
        [JsonIgnore]
        public int AgentId { get; set; }
        public string AgentCode { get; set; }
        public bool? Decision { get; set; }
    }
}