namespace Bookkeeping.Data.Repository.Interfaces
{
    public interface IAgentTaskFilter
    {
        int Offset { get; set; }
        int Limit { get; set; }
        int AgentId { get; set; }
        bool IsDone { get; set; }
    }
}
