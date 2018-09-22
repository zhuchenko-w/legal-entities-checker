using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BkTask = Bookkeeping.Data.Models.Task;
using TaskDecision = Bookkeeping.Data.Models.TaskDecision;
using ManagerTask = Bookkeeping.Data.Models.ManagerTask;
using AgentTask = Bookkeeping.Data.Models.AgentTask;
using TaskType = Bookkeeping.Data.Models.TaskType;
using Bookkeeping.Data.Repository.Interfaces;
using Bookkeeping.Data.Context;
using System.Data.Entity;
using Bookkeeping.Common.Exceptions;

namespace Bookkeeping.Data.Repository.Ef
{
    public class TaskRepository : ITaskRepository
    {
        public async Task<int> CreateTaskAsync(BkTask model)
        {
            using (var db = new BkDbContext())
            {
                var task = db.Tasks.Add(model);
                await db.SaveChangesAsync();

                return task.Id;
            }
        }

        public async Task<int> RemoveTaskAsync(int taskId)
        {
            using (var db = new BkDbContext())
            {
                var task = await db.Tasks.FirstOrDefaultAsync(p => p.Id == taskId);
                if (task != null)
                {
                    var resolutions = await db.Resolutions.Where(p => p.TaskId == taskId).ToListAsync();
                    if (resolutions.Any())
                    {
                        db.Resolutions.RemoveRange(resolutions);
                    }

                    db.Tasks.Remove(task);

                    return await db.SaveChangesAsync();
                }
            }

            return 0;
        }

        public async Task<int> GetTaskCountByAgentAsync(int agentId, bool isDone)
        {
            using (var db = new BkDbContext())
            {
                var today = DateTime.Now.Date;
                return await (
                    from task in db.Tasks
                    join resolution in db.Resolutions on task.Id equals resolution.TaskId
                    where resolution.AgentId == agentId 
                        && (!isDone && resolution.Decision == null || isDone && resolution.Decision != null)
                        && (!isDone || resolution.DateTimeDesktopUpdate >= today)
                    select task).CountAsync();
            }
        }

        public async Task<IList<ManagerTask>> GetListAsync(ITaskFilter taskFilter, DateTime? date)
        {
            using (var db = new BkDbContext())
            {
                var lowerName = taskFilter.Name?.ToLower();
                var lowerPurpose = taskFilter.Purpose?.ToLower();
                var agentIds = taskFilter.AgentIds == null ? new int[] { } : taskFilter.AgentIds;

                var filterByName = !string.IsNullOrEmpty(lowerName);
                var filterByPurpose = !string.IsNullOrEmpty(lowerPurpose);
                var filterByInn = !string.IsNullOrEmpty(taskFilter.Inn);
                var filterByAgents = agentIds.Length > 0;
                var filterByDate = date.HasValue;

                var tasksQuery = (
                    from task in db.Tasks
                    join resolution in db.Resolutions on task.Id equals resolution.TaskId
                    where task.IsArchive == taskFilter.IsArchive
                        && (!filterByName || task.Name != null && task.Name.ToLower().Contains(lowerName))
                        && (!filterByPurpose || task.PurposeOfPayment != null && task.PurposeOfPayment.ToLower().Contains(lowerPurpose))
                        && (!filterByInn || task.Inn != null && task.Inn.Contains(taskFilter.Inn))
                        && (!filterByAgents || agentIds.Contains(resolution.AgentId))
                        && (!filterByDate || task.Date == date.Value)
                    select task)
                    .Distinct()
                    .OrderByDescending(p => p.Date)
                    .Skip(taskFilter.Offset);

                if (taskFilter.Limit > 0)
                {
                    tasksQuery = tasksQuery.Take(taskFilter.Limit);
                }

                var tasks = (await tasksQuery.ToListAsync()).Select(p => new ManagerTask(p)).ToList();

                return await FillManagerTasksStatusesAndDecisions(db, tasks, taskFilter.Type);
            }
        }

        public async Task<IList<AgentTask>> GetListForAgentAsync(IAgentTaskFilter agentTaskFilter)
        {
            using (var db = new BkDbContext())
            {
                var resolutions = (await db.Resolutions
                    .Where(p => p.AgentId == agentTaskFilter.AgentId
                        && (p.Decision == null && !agentTaskFilter.IsDone || p.Decision != null && agentTaskFilter.IsDone)) 
                    .ToListAsync())
                    .Where(p => !agentTaskFilter.IsDone || p.DateTimeDesktopUpdate.Date == DateTime.Now.Date);

                var taskIds = resolutions.Select(p => p.TaskId);

                var tasksQuery = db.Tasks.Where(p => taskIds.Contains(p.Id))
                    .OrderByDescending(p => p.Date)
                    .Skip(agentTaskFilter.Offset);
                if (agentTaskFilter.Limit > 0)
                {
                    tasksQuery = tasksQuery.Take(agentTaskFilter.Limit);
                };

                return (await tasksQuery.ToListAsync())
                    .Select(t => {
                        return new AgentTask(t) {
                            Decision = resolutions.FirstOrDefault(r => r.TaskId == t.Id)?.Decision
                        };
                    }).ToList();
            }
        }

        public async Task<ManagerTask> GetByIdAsync(int taskId)
        {
            using (var db = new BkDbContext())
            {
                var task = new ManagerTask(await db.Tasks.FirstOrDefaultAsync(p => p.Id == taskId));
                await FillManagerTaskStatusAndDecision(db, task);

                return task;
            }
        }

        public async Task<int> MoveToArchiveAsync(int taskId)
        {
            using (var db = new BkDbContext())
            {
                var task = db.Tasks.FirstOrDefault(p => p.Id == taskId && !p.IsArchive);

                if (task != null)
                {
                    var resolutions = await db.Resolutions.Where(p => p.TaskId == taskId).ToListAsync();
                    var status = GetStatus(resolutions.Select(p => p.Decision));

                    if (status == TaskType.Green 
                        || status == TaskType.Red
                        || status == TaskType.Yellow)
                    {
                        task.IsArchive = true;
                        return await db.SaveChangesAsync();
                    }

                    throw new PublicException("Нельзя перенести в архив задание, по которому не все назначенные агенты вынесли резолюции");
                }
            }

            return 0;
        }

        //public async Task<BkTask> GetTaskByResolutionIdAsync(int resolutionId)
        //{
        //    using (var db = new BkDbContext())
        //    {
        //        var taskId = await db.Resolutions
        //            .Where(p => p.Id == resolutionId)
        //            .Select(p => p.TaskId)
        //            .FirstOrDefaultAsync();

        //        if (taskId > 0)
        //        {
        //            return await db.Tasks.FirstOrDefaultAsync(p => p.Id == taskId);
        //        }
        //    }

        //    return null;
        //}
        //public async Task<IList<BkTask>> SearchByInnAsync(ITaskFilter taskFilter, DateTime? date)
        //{
        //    using (var db = new BkDbContext())
        //    {
        //        var tasks = await db.Tasks
        //            .Where(p => p.Inn.Contains(taskFilter.Inn) 
        //                && p.IsArchive == taskFilter.IsArchive 
        //                && (date == null || p.Date == date.Value))
        //            .ToListAsync();

        //        return await FillTasksStatuses(db, tasks, taskFilter.Type);
        //    }
        //}

        //public async Task<IList<BkTask>> SearchByNameAsync(ITaskFilter taskFilter, DateTime? date)
        //{
        //    using (var db = new BkDbContext())
        //    {
        //        var lowerName = taskFilter.Name.ToLower();

        //        var tasks = await db.Tasks
        //            .Where(p => p.Name.ToLower().Contains(lowerName)
        //                && p.IsArchive == taskFilter.IsArchive
        //                && (date == null || p.Date == date.Value))
        //            .ToListAsync();

        //        return await FillTasksStatuses(db, tasks, taskFilter.Type);
        //    }
        //}

        //public Task<IList<TaskGroup>> GroupByDateAsync(ITaskFilter taskFilter)
        //{
        //    throw new NotImplementedException();
        //}

        //public Task<IList<TaskGroup>> GroupByDateWithFilterByInnAsync(ITaskFilter taskFilter)
        //{
        //    throw new NotImplementedException();
        //}

        //public Task<IList<TaskGroup>> GroupByDateWithFilterByNameAsync(ITaskFilter taskFilter)
        //{
        //    throw new NotImplementedException();
        //}

        //public async Task<IList<AgentTask>> GetInWorkAsync(int agentId)
        //{
        //    using (var db = new BkDbContext())
        //    {
        //        var today = DateTime.Now.Date;
        //        return await (
        //            from task in db.Tasks
        //            join resolution in db.Resolutions on task.Id equals resolution.TaskId
        //            where resolution.AgentId == agentId && resolution.Decision == null
        //                && resolution.DateTimeDesktopUpdate >= today
        //            select new AgentTask
        //            {
        //                ClientId = task.ClientId,
        //                Date = task.Date,
        //                Id = task.Id,
        //                Inn = task.Inn,
        //                IsArchive = task.IsArchive,
        //                Name = task.Name,
        //                TotalCount = task.TotalCount,
        //                PurposeOfPayment = task.PurposeOfPayment,
        //                Decision = resolution.Decision
        //            }).ToListAsync();
        //    }
        //}

        private async Task<IList<ManagerTask>> FillManagerTasksStatusesAndDecisions(BkDbContext db, List<ManagerTask> tasks, TaskType type)
        {
            foreach (var task in tasks)
            {
                await FillManagerTaskStatusAndDecision(db, task);
            }

            return type == TaskType.All ? tasks : tasks.Where(x => x.Status == type).ToList();
        }

        private async Task FillManagerTaskStatusAndDecision(BkDbContext db, ManagerTask task)
        {
            var decisions = await (
                from resolution in db.Resolutions
                join agent in db.Agents on resolution.AgentId equals agent.Id
                where resolution.TaskId == task.Id
                select new TaskDecision
                {
                    AgentId = agent.Id,
                    AgentCode = agent.Code,
                    Decision = resolution.Decision
                }).ToListAsync();

            task.Status = GetStatus(decisions.Select(p => p.Decision));
            task.Decisions = decisions;
        }

        private TaskType GetStatus(IEnumerable<bool?> decisions)
        {
            if (decisions.Any(x => x == null))
                return TaskType.NoStatus;

            if (decisions.All(x => x == true))
                return TaskType.Green;

            if (decisions.All(x => x == false))
                return TaskType.Red;

            return TaskType.Yellow;
        }
    }
}