using Bookkeeping.Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bookkeeping.Data.Repository.Interfaces
{
    public interface IResolutionRepository
    {
        Task<Resolution> GetByIdAsync(int resolutionId);
        Task<IEnumerable<Resolution>> GetListByTaskIdAsync(int taskId);
        Task<AgentResolutionModel> GetByTaskAndAgentIdsAsync(int taskId, int agentId);
        Task<int> CreateResolutionAsync(Resolution model);
        Task<bool> UpdateResolutionAsync(Resolution model);
        Task<int> RemoveByAgentIdAsync(int agentId);
    }
}