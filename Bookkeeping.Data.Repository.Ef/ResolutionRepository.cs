using Bookkeeping.Data.Repository.Interfaces;
using System;
using Bookkeeping.Data.Models;
using System.Threading.Tasks;
using Bookkeeping.Data.Context;
using System.Linq;
using System.Data.Entity;
using System.Collections.Generic;
using Bookkeeping.Common.Exceptions;

namespace Bookkeeping.Data.Repository.Ef
{
    public class ResolutionRepository : IResolutionRepository
    {
        public async Task<Resolution> GetByIdAsync(int resolutionId)
        {
            using (var db = new BkDbContext())
            {
                return await db.Resolutions.FirstOrDefaultAsync(p => p.Id == resolutionId);
            }
        }

        public async Task<AgentResolutionModel> GetByTaskAndAgentIdsAsync(int taskId, int agentId)
        {
            using (var db = new BkDbContext())
            {
                var agentResolution = await (
                    from task in db.Tasks
                    join resolution in db.Resolutions on task.Id equals resolution.TaskId
                    where resolution.AgentId == agentId && resolution.TaskId == taskId 
                    select new AgentResolutionModel {
                        Id = resolution.Id,
                        AgentId = resolution.AgentId,
                        Decision = resolution.Decision,
                        PurposeOfPaymentComment = resolution.PurposeOfPaymentComment,
                        ImageData = resolution.ImageData,
                        ImageName = resolution.ImageName,
                        MimeType = resolution.MimeType,
                        Note = resolution.Note,
                        DateTimeDesktopUpdate = resolution.DateTimeDesktopUpdate,
                        TaskId = resolution.TaskId,
                        Inn = task.Inn,
                        PurposeOfPayment = task.PurposeOfPayment
                    }).FirstOrDefaultAsync();

                if(agentResolution == null)
                {
                    throw new PublicException("Резолюция не найдена");
                }

                if (agentResolution.Decision != null && agentResolution.DateTimeDesktopUpdate.Date != DateTime.Now.Date)
                {
                    throw new PublicException("Время редактирования резолюции истекло");
                }

                return agentResolution;
            }
        }

        public async Task<IEnumerable<Resolution>> GetListByTaskIdAsync(int taskId)
        {
            using (var db = new BkDbContext())
            {
                return await db.Resolutions.Include("Agent")
                    .Where(p => p.TaskId == taskId)
                    .ToListAsync();
            }
        }

        public async Task<int> CreateResolutionAsync(Resolution model)
        {
            using (var db = new BkDbContext())
            {
                var resolution = db.Resolutions.Add(model);
                await db.SaveChangesAsync();

                return resolution.Id;
            }
        }

        public async Task<bool> UpdateResolutionAsync(Resolution resolution)
        {
            using (var db = new BkDbContext())
            {
                var existingResolution = await db.Resolutions.FirstOrDefaultAsync(p => p.Id == resolution.Id);

                if (existingResolution != null)
                {
                    if(existingResolution.Decision != null && existingResolution.DateTimeDesktopUpdate.Date != DateTime.Now.Date)
                    {
                        throw new PublicException("Время редактирования резолюции истекло");
                    }

                    existingResolution.DateResolution = resolution.DateResolution;
                    existingResolution.DateTimeMobileUpdate = resolution.DateTimeMobileUpdate;
                    existingResolution.Decision = resolution.Decision;
                    existingResolution.ImageData = resolution.ImageData;
                    existingResolution.ImageName = resolution.ImageName;
                    existingResolution.MimeType = resolution.MimeType;
                    existingResolution.Note = resolution.Note;
                    existingResolution.PurposeOfPaymentComment = resolution.PurposeOfPaymentComment;

                    await db.SaveChangesAsync();

                    return true;
                }

                throw new PublicException("Резолюция была удалена");
            }
        }

        public async Task<int> RemoveByAgentIdAsync(int agentId)
        {
            using (var db = new BkDbContext())
            {
                var resolutions = await db.Resolutions.Where(p => p.AgentId == agentId).ToListAsync();

                if (resolutions.Any())
                {
                    db.Resolutions.RemoveRange(resolutions);
                    await db.SaveChangesAsync();
                    return resolutions.Count;
                }

                return 0;
            }
        }
    }
}
