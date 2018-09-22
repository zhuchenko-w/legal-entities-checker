using Bookkeeping.Common.Exceptions;
using Bookkeeping.Common.Interfaces;
using Bookkeeping.Data.Repository.Interfaces;
using Bookkeeping.Models;
using Bookkeeping.WebUi.Models;
using Nelibur.ObjectMapper;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;
using BkTask = Bookkeeping.Data.Models.Task;
using ManagerTask = Bookkeeping.Data.Models.ManagerTask;
using Bookkeeping.BusinessLogic.Interfaces;
using Bookkeeping.Data.Models.Identity;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Bookkeeping.Data.Repository.Ef;
using System.Web;
using System.Security.Claims;

namespace Bookkeeping.WebUi.Controllers
{
    [BkAuthorize(Roles = UserRoleNames.Manager)]
    public class ManagerController : BaseController
    {
        private readonly int _pageSize;
        private readonly ITaskRepository _taskRepository;
        private readonly IAgentRepository _agentRepository;
        private readonly IResolutionRepository _resolutionRepository;
        private readonly IT1000Logic _companyServiceLogic;

        public ManagerController(ITaskRepository taskRepository, 
            IAgentRepository agentRepository, 
            IResolutionRepository resolutionRepository,
            IT1000Logic companyServiceLogic,
            ISettingsManager settingsManager,
            ILogger logger) : base (logger)
        {
            _taskRepository = taskRepository;
            _agentRepository = agentRepository;
            _resolutionRepository = resolutionRepository;
            _companyServiceLogic = companyServiceLogic;
            _pageSize = settingsManager.GetValue<int>("ManagerTasksPageSize");
        }

        public async Task<ActionResult> Tasks(TaskFilter taskFilter, DateTime? date)
        {
            if (taskFilter.Offset == 0 && taskFilter.Limit == 0)
            {
                taskFilter.Limit = _pageSize;
            }

            var model = await GetFilteredTasks(taskFilter, date, false);
            model.Agents = await _agentRepository.GetAllAync();
            model.PageSize = _pageSize;

            return View(model);
        }

        [HttpPost]
        public async Task<JsonResult> GetTasks(TaskFilter taskFilter, DateTime? date)
        {
            return await RunWithResult(async () => {
                var model = await GetFilteredTasks(taskFilter, date, true);
                return model.Tasks.Count == 0 && taskFilter.Offset > 0 ? "" : RenderViewToString("_TaskList", model);
            }, "При получении списка заданий произошла ошибка");
        }

        [HttpPost]
        public async Task<JsonResult> GetResolutions(int taskId)
        {
            return await RunWithResult(async () =>
            {
                var resolutions = await _resolutionRepository.GetListByTaskIdAsync(taskId);
                var model = TinyMapper.Map<List<ResolutionViewModel>>(resolutions);
                return RenderViewToString("_TaskResolutionsList", model);
            }, "При получении релозюций произошла ошибка");
        }

        [HttpPost]
        public async Task<JsonResult> CreateTask(CreateTaskViewModel task)
        {
            return await RunWithResult(async () =>
            {
                var taskId = await CreateNewTask(task);
                var model = await _taskRepository.GetByIdAsync(taskId);
                return RenderViewToString("_TaskItem", model);
            }, "При сохранении задания произошла ошибка");
        }

        [HttpPost]
        public async Task<JsonResult> RemoveTask(int taskId)
        {
            return await Run(async () => { await _taskRepository.RemoveTaskAsync(taskId); }, "При удалении задания произошла ошибка");
        }

        [HttpPost]
        public async Task<JsonResult> MoveTaskToArchive(int taskId)
        {
            return await Run(async () => { await _taskRepository.MoveToArchiveAsync(taskId); }, "При перемещении задания в архив произошла ошибка");
        }

        [HttpPost]
        public async Task<JsonResult> RepeatTask(int taskId)
        {
            return await Run(async () => {
                var task = await _taskRepository.GetByIdAsync(taskId);

                await CreateNewTask(new CreateTaskViewModel
                {
                    Inn = task.Inn,
                    PurposeOfPayment = task.PurposeOfPayment,
                    AgentIds = task.Decisions.Select(p => p.AgentId).ToArray()
                });
            }, "При создании повтора задания произошла ошибка");
        }

        [HttpPost]
        public async Task<JsonResult> SearchNameByInn(string inn)
        {
            return await RunWithResult(async () =>
            {
                var userManager = HttpContext.GetOwinContext().GetUserManager<BkUserManager>();
                var userId = User.Identity.GetUserId();
                var dbsidClaim = (await userManager.GetClaimsAsync(userId)).FirstOrDefault(p => p.Type == Common.Constants.DbsidClaimType);
                if (!string.IsNullOrEmpty(dbsidClaim?.Value))
                {
                    try
                    {
                        var result = await _companyServiceLogic.GetCompanyNameByInn(inn, dbsidClaim.Value);

                        if (!string.Equals(result.NewDbsid, dbsidClaim.Value, StringComparison.InvariantCultureIgnoreCase))
                        {
                            await userManager.RemoveClaimAsync(userId, dbsidClaim);
                            await userManager.AddClaimAsync(userId, new Claim(Common.Constants.DbsidClaimType, result.NewDbsid));
                        }

                        return result.CompanyName;
                    }
                    catch(SessionExpiredException)
                    {
                        throw;
                    }
                    catch (PublicException ex)
                    {
                        _logger.Error("При получении имени ЮЛ по ИНН произошла ошибка", ex);
                        throw;
                    }
                    catch (Exception ex)
                    {
                        _logger.Error("При получении имени ЮЛ по ИНН произошла ошибка", ex);
                    }
                }
                else
                {
                    _logger.Error($"dbsid пользователя {User.Identity.GetUserId()} не найден");
                }

                return "";
            }, "При поиске имени ЮЛ по ИНН произошла ошибка");
        }

        private async Task<int> CreateNewTask(CreateTaskViewModel task)
        {
            if (string.IsNullOrEmpty(task.Inn) || 
                task.Inn.Length != Common.Constants.InnLengthIndividual && task.Inn.Length != Common.Constants.InnLengthLegal ||
                !(Common.Constants.InnRegex.IsMatch(task.Inn)))
            {
                throw new PublicException($"ИНН должен coстоять из {Common.Constants.InnLengthIndividual} или {Common.Constants.InnLengthLegal} цифр");
            }
            if (task.AgentIds == null || task.AgentIds.Length == 0)
            {
                throw new PublicException("Для задания не назначены агенты");
            }
            
            var newTask = new BkTask
            {
                Date = DateTime.Now,
                Inn = task.Inn,
                Name = task.Name,
                PurposeOfPayment = task.PurposeOfPayment,
                IsArchive = false,
                TotalCount = task.AgentIds.Length
            };

            var userManager = HttpContext.GetOwinContext().GetUserManager<BkUserManager>();
            var userId = User.Identity.GetUserId();
            var clientIdClaim = (await userManager.GetClaimsAsync(userId)).FirstOrDefault(p => p.Type == Common.Constants.ClientIdClaimType);
            if (!string.IsNullOrEmpty(clientIdClaim?.Value) && int.TryParse(clientIdClaim.Value, out int clientId))
            {
                newTask.ClientId = clientId;
            }

            var taskId = await _taskRepository.CreateTaskAsync(newTask);

            foreach (var agentId in task.AgentIds)
            {
                var resolution = new Data.Models.Resolution
                {
                    AgentId = agentId,
                    TaskId = taskId,
                    Decision = null,
                    Note = string.Empty,
                    ImageData = string.Empty,
                    ImageName = string.Empty,
                    DateTimeDesktopUpdate = DateTime.Now
                };
                await _resolutionRepository.CreateResolutionAsync(resolution);
            }

            return taskId;
        }

        private async Task<ManagerTasksViewModel> GetFilteredTasks(TaskFilter taskFilter, DateTime? date, bool throwOnError)
        {
            try
            {
                if (taskFilter == null)
                {
                    taskFilter = new TaskFilter
                    {
                        IsArchive = false
                    };
                }

                var tasks = await _taskRepository.GetListAsync(taskFilter, date);

                return new ManagerTasksViewModel
                {
                    IsArchived = taskFilter.IsArchive,
                    Tasks = tasks
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
                    _logger.Error("При получении списка заданий произошла ошибка", ex);
                    return new ManagerTasksViewModel
                    {
                        IsArchived = taskFilter.IsArchive,
                        Tasks = new List<ManagerTask>(),
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
                    var message = "При получении списка заданий произошла ошибка";
                    _logger.Error(message, ex);
                    return new ManagerTasksViewModel
                    {
                        IsArchived = taskFilter.IsArchive,
                        Tasks = new List<ManagerTask>(),
                        TasksError = message
                    };
                }
            }
        }
    }
}