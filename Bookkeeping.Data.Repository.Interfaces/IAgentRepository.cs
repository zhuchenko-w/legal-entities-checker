using Bookkeeping.Data.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bookkeeping.Data.Repository.Interfaces
{
    public interface IAgentRepository
    {
        Task<int> CreateAgentAync(string code, string password);

        Task<IList<Agent>> GetAllAync(bool exceptLocked = false);

        Task<Agent> GetByIdAync(int agentId);

        Task<Agent> GetByCodeAync(string code);

        Task<int> GetIdByCodeAync(string code);

        Task<bool> UpdateCodeAndPasswordAsync(int agentId, string code, string password);

        Task<bool> SetLockedAsync(int agentId, bool isLocked);

        Task<bool> UpdateLastSeenDateAsync(int agentId, DateTime lastSeen);

        Task<string> RemoveAgentAync(int agentId);
    }
}