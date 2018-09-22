using Bookkeeping.Data.Models;

namespace Bookkeeping.Data.Repository.Interfaces
{
    public interface ITaskFilter
    {
        int Offset { get; set; }
        int Limit { get; set; }
        TaskType Type { get; set; }
        string Inn { get; set; }
        string Name { get; set; }
        string Purpose { get; set; }
        int[] AgentIds { get; set; }
        bool IsArchive { get; set; }
    }
}
