using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BkTask = Bookkeeping.Data.Models.Task;
using ManagerTask = Bookkeeping.Data.Models.ManagerTask;
using AgentTask = Bookkeeping.Data.Models.AgentTask;

namespace Bookkeeping.Data.Repository.Interfaces
{
    public interface ITaskRepository
    {
        Task<IList<ManagerTask>> GetListAsync(ITaskFilter taskFilter, DateTime? date);
        Task<IList<AgentTask>> GetListForAgentAsync(IAgentTaskFilter agentTaskFilter);
        Task<ManagerTask> GetByIdAsync(int taskId);
        Task<int> GetTaskCountByAgentAsync(int agentId, bool isDone);
        Task<int> CreateTaskAsync(BkTask model);
        Task<int> RemoveTaskAsync(int taskId);
        Task<int> MoveToArchiveAsync(int taskId);

        //Task<BkTask> GetTaskByResolutionIdAsync(int resolutionId);
        //Task<IList<BkTask>> SearchByInnAsync(ITaskFilter taskFilter, DateTime? date);
        //Task<IList<BkTask>> SearchByNameAsync(ITaskFilter taskFilter, DateTime? date);
        //Task<IList<TaskGroup>> GroupByDateWithFilterByInnAsync(ITaskFilter taskFilter);
        //Task<IList<TaskGroup>> GroupByDateWithFilterByNameAsync(ITaskFilter taskFilter);
        //Task<IList<TaskGroup>> GroupByDateAsync(ITaskFilter taskFilter);
    }
}