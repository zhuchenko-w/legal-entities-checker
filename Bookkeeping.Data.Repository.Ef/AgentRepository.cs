using Bookkeeping.Data.Repository.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bookkeeping.Data.Models;
using Bookkeeping.Data.Context;
using System.Linq;
using System.Data.Entity;
using Bookkeeping.Common.Exceptions;

namespace Bookkeeping.Data.Repository.Ef
{
    public class AgentRepository : IAgentRepository//todo: caching
    {
        public async Task<int> CreateAgentAync(string code, string password)
        {
            using (var db = new BkDbContext())
            {
                var existingAgent = await db.Agents.FirstOrDefaultAsync(p => p.Code == code);
                if (existingAgent != null)
                {
                    throw new PublicException($"Агент c таким логином уже существует");
                }

                var agent = db.Agents.Add(new Agent
                {
                    Code = code,
                    Password = password
                });
                await db.SaveChangesAsync();

                return agent.Id;
            }
        }

        public async Task<IList<Agent>> GetAllAync(bool exceptLocked = false)
        {
            using (var db = new BkDbContext())
            {
                return await db.Agents
                    .Where(p => !exceptLocked || !p.IsLocked.HasValue || !p.IsLocked.Value)
                    .OrderByDescending(p => p.LastSeen)
                    .ThenBy(p => p.Code)
                    .ToListAsync();
            }
        }

        public async Task<Agent> GetByIdAync(int agentId)
        {
            using (var db = new BkDbContext())
            {
                var agent = await db.Agents.FirstOrDefaultAsync(p => p.Id == agentId);
                if (agent == null)
                {
                    throw new PublicException($"Агент не найден");
                }

                return agent;
            }
        }

        public async Task<Agent> GetByCodeAync(string code)
        {
            using (var db = new BkDbContext())
            {
                var agent = await db.Agents.FirstOrDefaultAsync(p => p.Code == code);
                if (agent == null)
                {
                    throw new PublicException($"Агент не найден");
                }

                return agent;
            }
        }

        public async Task<int> GetIdByCodeAync(string code)
        {
            using (var db = new BkDbContext())
            {
                var agentId = (await db.Agents.FirstOrDefaultAsync(p => p.Code == code))?.Id;
                if (!agentId.HasValue)
                {
                    throw new PublicException($"Агент не найден");
                }

                return agentId.Value;
            }
        }

        public async Task<bool> UpdateCodeAndPasswordAsync(int agentId, string code, string password)
        {
            return await UpdateAgent(agentId, (p) => {
                p.Code = code;
                p.Password = password;
            });
        }

        public async Task<bool> SetLockedAsync(int agentId, bool isLocked)
        {
            return await UpdateAgent(agentId, (p) => {
                p.IsLocked = isLocked;
            });
        }

        public async Task<bool> UpdateLastSeenDateAsync(int agentId, DateTime lastSeen)
        {
            return await UpdateAgent(agentId, (p) => {
                p.LastSeen = lastSeen;
            });
        }

        public async Task<string> RemoveAgentAync(int agentId)
        {
            using (var db = new BkDbContext())
            {
                var existingAgent = await db.Agents.FirstOrDefaultAsync(p => p.Id == agentId);
                if (existingAgent != null)
                {
                    db.Agents.Remove(existingAgent);
                    await db.SaveChangesAsync();
                }

                return existingAgent.Code;
            }
        }

        private async Task<bool> UpdateAgent(int agentId, Action<Agent> updateFunc)
        {
            using (var db = new BkDbContext())
            {
                var existingAgent = await db.Agents.FirstOrDefaultAsync(p => p.Id == agentId);
                if (existingAgent == null)
                {
                    throw new PublicException($"Агент не найден");
                }

                updateFunc(existingAgent);

                return (await db.SaveChangesAsync()) > 0;
            }
        }
    }
}
