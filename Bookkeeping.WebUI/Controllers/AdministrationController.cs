using Bookkeeping.Common.Exceptions;
using Bookkeeping.Common.Interfaces;
using Bookkeeping.Data.Models;
using Bookkeeping.Data.Repository.Interfaces;
using Bookkeeping.WebUi.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.AspNet.Identity.Owin;
using System.Web;
using Agent = Bookkeeping.Data.Models.Agent;
using Bookkeeping.Data.Repository.Ef;
using Bookkeeping.Data.Models.Identity;

namespace Bookkeeping.WebUi.Controllers
{
    [BkAuthorize(Roles = UserRoleNames.Manager)]
    public class AdministrationController : BaseController
    {
        private readonly IAgentRepository _agentRepository;
        private readonly IResolutionRepository _resolutionRepository;

        public AdministrationController(IAgentRepository agentRepository,
            IResolutionRepository resolutionRepository,
            ILogger logger) : base (logger)
        {
            _agentRepository = agentRepository;
            _resolutionRepository = resolutionRepository;
        }

        public async Task<ActionResult> Agents()
        {
            var agents = await GetAgents();
            return View(agents);
        }

        [HttpPost]
        public async Task<JsonResult> CreateAgent(AgentCredentialsViewModel model)
        {
            return await RunWithResult(async () => {
                ValidateAgentCredentials(model);

                var userManager = HttpContext.GetOwinContext().GetUserManager<BkUserManager>();
                var roleManager = HttpContext.GetOwinContext().GetUserManager<BkRoleManager>();

                var existingUser = await userManager.FindByNameAsync(model.Code);
                if (existingUser != null)
                {
                    throw new PublicException($"Пользователь с именем {model.Code} уже существует");
                }

                if (!roleManager.RoleExists(UserRoleNames.Agent))
                {
                    var roleCreationResult = await roleManager.CreateAsync(new BkRole(UserRoleNames.Agent));
                    if (!roleCreationResult.Succeeded)
                    {
                        throw new PublicException(string.Join(". ", roleCreationResult.Errors));
                    }
                }

                var user = new BkUser { UserName = model.Code };
                var userCreationResult = await userManager.CreateAsync(user, model.Password);
                if (!userCreationResult.Succeeded)
                {
                    throw new PublicException(string.Join(". ", userCreationResult.Errors));
                }

                var addToRoleResult = await userManager.AddToRoleAsync(user.Id, UserRoleNames.Agent);
                if (!addToRoleResult.Succeeded)
                {
                    throw new PublicException(string.Join(". ", addToRoleResult.Errors));
                }

                var agentId = await _agentRepository.CreateAgentAync(model.Code, model.Password);

                return new Agent {
                    Id = agentId,
                    Code = model.Code,
                    Password = model.Password
                };
            }, "При создании агента произошла ошибка");
        }

        [HttpPost]
        public async Task<JsonResult> RemoveAgent(int agentId)
        {
            return await RunWithResult(async () => {
                string result = null;
                var removedResolutionsCount = await _resolutionRepository.RemoveByAgentIdAsync(agentId);
                var code = await _agentRepository.RemoveAgentAync(agentId);

                var userManager = HttpContext.GetOwinContext().GetUserManager<BkUserManager>();
                var user = await userManager.FindByNameAsync(code);
                if (user != null)
                {
                    if (User.Identity.GetUserId() == user.Id)
                    {
                        result = Url.Action("logout", "account");
                    }

                    var errors = new List<string>();
                    var updateSeurityStampResult = await userManager.UpdateSecurityStampAsync(user.Id);
                    if (!updateSeurityStampResult.Succeeded)
                    {
                        errors.AddRange(updateSeurityStampResult.Errors);
                    }

                    var deleteResult = await userManager.DeleteAsync(user);
                    if(!deleteResult.Succeeded)
                    {
                        errors.AddRange(deleteResult.Errors);
                        throw new Exception("Ошибка при удалении учетной записи агента." + Environment.NewLine +  string.Join(". ", errors));
                    }
                }

                return result;
            }, "При удалении агента и резолюций произошла ошибка");
        }

        [HttpPost]
        public async Task<JsonResult> EditAgent(int agentId, AgentCredentialsViewModel model)
        {
            return await Run(async () => {
                ValidateAgentCredentials(model);

                var agent = await _agentRepository.GetByIdAync(agentId);
                if(agent.Code == model.Code && agent.Password == model.Password)
                {
                    return;
                }

                var userManager = HttpContext.GetOwinContext().GetUserManager<BkUserManager>();
                var user = await userManager.FindByNameAsync(agent.Code);
                if (user != null)
                {
                    var errors = new List<string>();
                    if(agent.Code != model.Code)
                    {
                        user.UserName = model.Code;
                        var updateResult = await userManager.UpdateAsync(user);
                        if (!updateResult.Succeeded)
                        {
                            errors.AddRange(updateResult.Errors);
                        }
                    }
                    if (agent.Password != model.Password)
                    {
                        var changePasswordResult = await userManager.ChangePasswordAsync(user.Id, agent.Password, model.Password);
                        if (!changePasswordResult.Succeeded)
                        {
                            errors.AddRange(changePasswordResult.Errors);
                        }
                        var updateSeurityStampResult = await userManager.UpdateSecurityStampAsync(user.Id);
                        if (!updateSeurityStampResult.Succeeded)
                        {
                            errors.AddRange(updateSeurityStampResult.Errors);
                        }
                    }                    
                    await _agentRepository.UpdateCodeAndPasswordAsync(agentId, model.Code, model.Password);

                    if (errors.Count > 0)
                    {
                        throw new PublicException("Во время редактирования агента произошли ошибки: " + 
                            Environment.NewLine + string.Join("; ", errors));
                    }
                }
                else
                {
                    throw new PublicException("Пользователь не найден");
                }
            }, "При редактировании данных агента произошла ошибка");
        }

        [HttpPost]
        public async Task<JsonResult> SetAgentLockState(int agentId, bool isLocked)
        {
            return await Run(async () => {
                await _agentRepository.SetLockedAsync(agentId, isLocked);
                var agent = await _agentRepository.GetByIdAync(agentId);
                if (agent?.IsLocked ?? false)
                {
                    var userManager = HttpContext.GetOwinContext().GetUserManager<BkUserManager>();
                    var user = await userManager.FindByNameAsync(agent.Code);
                    if (user != null)
                    {
                        var updateSeurityStampResult = await userManager.UpdateSecurityStampAsync(user.Id);
                        if (!updateSeurityStampResult.Succeeded)
                        {
                            throw new PublicException("Не удалось ограничить доступ агенту." +
                                Environment.NewLine + string.Join("; ", updateSeurityStampResult.Errors));
                        }
                    }
                }
            }, "При изменении статуса агента произошла ошибка");
        }

        private async Task<AgentsViewModel> GetAgents()
        {
            try
            {
                var agents = await _agentRepository.GetAllAync();
                return new AgentsViewModel
                {
                    Agents = agents
                };
            }
            catch (PublicException ex)
            {
                _logger.Error("При получении списка агентов произошла ошибка", ex);
                return new AgentsViewModel
                {
                    Agents = new List<Agent>(),
                    AgentsError = ex.Message
                };
            }
            catch (Exception ex)
            {
                var message = "При получении списка агентов произошла ошибка";
                _logger.Error(message, ex);
                return new AgentsViewModel
                {
                    Agents = new List<Agent>(),
                    AgentsError = message
                };
            }
        }

        private void ValidateAgentCredentials(AgentCredentialsViewModel credentials)
        {
            if (string.IsNullOrEmpty(credentials.Code))
            {
                throw new PublicException("Не указан логин агента");
            }
            if (string.IsNullOrEmpty(credentials.Password))
            {
                throw new PublicException("Не указан пароль агента");
            }
            if (credentials.Password.Length < Common.Constants.MinPasswordLength)
            {
                throw new PublicException($"Минимальная длина пароля - {Common.Constants.MinPasswordLength} символов");
            }
        }
    }
}