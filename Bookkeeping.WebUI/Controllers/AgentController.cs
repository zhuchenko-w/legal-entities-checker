using Bookkeeping.Common.Exceptions;
using Bookkeeping.Common.Interfaces;
using Bookkeeping.Data.Models;
using Bookkeeping.Data.Models.Identity;
using Bookkeeping.Data.Repository.Interfaces;
using Bookkeeping.WebUi.Models;
using Nelibur.ObjectMapper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Web.Mvc;
using AgentTask = Bookkeeping.Data.Models.AgentTask;

namespace Bookkeeping.WebUi.Controllers
{
    [BkAuthorize(Roles = UserRoleNames.Agent)]
    public class AgentController : BaseController
    {
        private readonly int _pageSize;
        private readonly ITaskRepository _taskRepository;
        private readonly IResolutionRepository _resolutionRepository;
        private readonly IAgentRepository _agentRepository;

        public AgentController(ITaskRepository taskRepository,
            IAgentRepository agentRepository,
            IResolutionRepository resolutionRepository,
            ISettingsManager settingsManager,
            ILogger logger) : base (logger)
        {
            _taskRepository = taskRepository;
            _resolutionRepository = resolutionRepository;
            _agentRepository = agentRepository;
            _pageSize = settingsManager.GetValue<int>("AgentTasksPageSize");
        }

        [HttpGet]
        public async Task<ActionResult> Tasks(AgentTaskFilterViewModel taskFilter)
        {
            if (taskFilter == null)
            {
                taskFilter = new AgentTaskFilterViewModel();
            }

            if (taskFilter.Offset == 0 && taskFilter.Limit == 0)
            {
                taskFilter.Limit = _pageSize;
            }

            var model = await GetTasksInternal(taskFilter, false);
            model.PageSize = _pageSize;

            return View(model);
        }

        [HttpPost]
        public async Task<JsonResult> GetTasks(AgentTaskFilterViewModel taskFilter)
        {
            return await RunWithResult(async () =>
            {
                var model = await GetTasksInternal(taskFilter, true);
                return new
                {
                    tasksHtml = model.Tasks.Count == 0 && taskFilter.Offset > 0 ? "" : RenderViewToString("_TaskList", model.Tasks),
                    inWorkCount = model.InWorkCount,
                    doneCount = model.DoneCount
                };
            }, "При получении заданий произошла ошибка");
        }

        [HttpGet]
        public async Task<ActionResult> Resolution(int taskId, bool isDone)
        {
            try
            {
                var agentId = await _agentRepository.GetIdByCodeAync(User.Identity.Name);
                var resolution = await _resolutionRepository.GetByTaskAndAgentIdsAsync(taskId, agentId);
                var model = TinyMapper.Map<AgentResolutionViewModel>(resolution);

                return View(model);
            }
            catch (PublicException ex)
            {
                _logger.Error("При получении резолюции произошла ошибка", ex);
                return RedirectToAction("Tasks", new AgentTaskFilterViewModel
                {
                    IsDone = isDone,
                    ResolutionError = ex.Message
                });
            }
            catch (Exception ex)
            {
                var errorMessage = "При получении резолюции произошла ошибка";
                _logger.Error(errorMessage, ex);
                return RedirectToAction("Tasks", new AgentTaskFilterViewModel
                {
                    IsDone = isDone,
                    ResolutionError = errorMessage
                });
            }
        }

        [HttpPost]
        public async Task<JsonResult> SaveResolution(AgentResolutionViewModel model)
        {
            return await RunWithResult(async () =>
            {
                //TODO: validate model
                var now = DateTime.Now;

                var resolution = TinyMapper.Map<Resolution>(model);
                resolution.DateResolution = now;
                resolution.DateTimeMobileUpdate = now;

                if (model.ImageFile != null)
                {
                    using (var reader = new BinaryReader(model.ImageFile.InputStream))
                    {
                        resolution.ImageData = Convert.ToBase64String(reader.ReadBytes(model.ImageFile.ContentLength));
                    }
                    resolution.ImageName = model.ImageFile.FileName;
                    resolution.MimeType = model.ImageFile.ContentType;
                }

                await _resolutionRepository.UpdateResolutionAsync(resolution);

                var urlHelper = new UrlHelper(ControllerContext.RequestContext);
                return urlHelper.Action("Tasks", new AgentTaskFilterViewModel
                {
                    IsDone = true
                });
            }, "При сохранении резолюции произошла ошибка");
        }

        private async Task<AgentTasksViewModel> GetTasksInternal(AgentTaskFilterViewModel filter, bool throwOnError)
        {
            var fullFilter = TinyMapper.Map<AgentTaskFilter>(filter);
            fullFilter.AgentId = await _agentRepository.GetIdByCodeAync(User.Identity.Name);

            try
            {
                var tasks = await _taskRepository.GetListForAgentAsync(fullFilter);
                var inWorkCount = await _taskRepository.GetTaskCountByAgentAsync(fullFilter.AgentId, false);
                var doneCount = await _taskRepository.GetTaskCountByAgentAsync(fullFilter.AgentId, true);

                return new AgentTasksViewModel
                {
                    Tasks = tasks,
                    InWorkCount = inWorkCount,
                    DoneCount = doneCount,
                    IsDone = filter.IsDone,
                    ResolutionError = filter.ResolutionError
                };
            }
            catch (PublicException ex)
            {
                if (throwOnError)
                {
                    throw;
                }
                else
                {
                    _logger.Error("При получении списка заданий для агента произошла ошибка", ex);
                    return new AgentTasksViewModel
                    {
                        Tasks = new List<AgentTask>(),
                        InWorkCount = 0,
                        DoneCount = 0,
                        IsDone = filter.IsDone,
                        ResolutionError = filter.ResolutionError,
                        TasksError = ex.Message
                    };
                }
            }
            catch (Exception ex)
            {
                if (throwOnError)
                {
                    throw;
                }
                else
                {
                    _logger.Error($"При получении списка заданий для агента {fullFilter.AgentId} произошла ошибка", ex);
                    return new AgentTasksViewModel
                    {
                        Tasks = new List<AgentTask>(),
                        InWorkCount = 0,
                        DoneCount = 0,
                        IsDone = filter.IsDone,
                        ResolutionError = filter.ResolutionError,
                        TasksError = "При получении списка заданий для произошла ошибка"
                    };
                }
            }
        }
    }
}