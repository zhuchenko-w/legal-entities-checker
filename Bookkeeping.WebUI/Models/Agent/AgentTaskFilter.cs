using Bookkeeping.Data.Repository.Interfaces;

namespace Bookkeeping.WebUi.Models
{
    public class AgentTaskFilter : AgentTaskFilterViewModel, IAgentTaskFilter
    {
        public int AgentId { get; set; }
    }
}