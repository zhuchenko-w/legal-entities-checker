using Bookkeeping.Common.Exceptions;
using Bookkeeping.Common.Interfaces;
using Bookkeeping.Data.Models;
using Bookkeeping.WebUi.Models;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using System.Web;
using System;
using System.Linq;
using Microsoft.AspNet.Identity.Owin;
using Bookkeeping.Data.Repository.Ef;
using Bookkeeping.Data.Repository.Interfaces;
using Bookkeeping.Data.Models.Identity;
using Bookkeeping.BusinessLogic.Interfaces;
using System.Security.Claims;
using System.Collections.Generic;

namespace Bookkeeping.WebUi.Controllers
{
    public class AccountController : BaseController
    {
        private readonly IAgentRepository _agentRepository;
        private readonly IT1000Logic _t1000Logic;
        private readonly string _defaultControllerName;

        public AccountController(IAgentRepository agentRepository,
            IT1000Logic t1000Logic,
            ISettingsManager settingsManager,
            ILogger logger) : base (logger)
        {
            _agentRepository = agentRepository;
            _t1000Logic = t1000Logic;
            _defaultControllerName = settingsManager.GetValue("DefaultRouteControllerName", "Manager");
        }

        [HttpGet]
        public ActionResult Login(string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                if (!string.IsNullOrEmpty((returnUrl ?? "").Trim(" /".ToCharArray())) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }
                if (User.IsInRole(UserRoleNames.Manager))
                {
                    return RedirectToAction("Tasks", "Manager");
                }
                if (User.IsInRole(UserRoleNames.Agent))
                {
                    return RedirectToAction("Tasks", "Agent");
                }

                return new HttpStatusCodeResult(System.Net.HttpStatusCode.Forbidden);
            }

            return View(model: returnUrl);
        }

        [HttpPost]
        public async Task<JsonResult> Login(LoginViewModel model)
        {
            return await RunWithResult(async () => {
                var authManager = HttpContext.GetOwinContext().Authentication;
                var userManager = HttpContext.GetOwinContext().GetUserManager<BkUserManager>();

                var user = await userManager.FindAsync(model.Username, model.Password);
                if (user != null)
                {
                    var agentId = 0;
                    var hasRole = false;
                    var roles = new List<string>();
                    if (await userManager.IsInRoleAsync(user.Id, UserRoleNames.Agent))
                    {
                        roles.Add(UserRoleNames.Agent);

                        var agent = await _agentRepository.GetByCodeAync(user.UserName);
                        if (agent.IsLocked.HasValue && agent.IsLocked.Value)
                        {
                            throw new PublicException("Аккаунт заблокирован");
                        }

                        agentId = agent.Id;
                        hasRole = true;
                    }
                    if (await userManager.IsInRoleAsync(user.Id, UserRoleNames.Manager))
                    {
                        roles.Add(UserRoleNames.Manager);
                        hasRole = true;
                    }
                    
                    if(!hasRole)
                    {
                        throw new PublicException("Недостаточно прав");
                    }

                    await userManager.UpdateSecurityStampAsync(user.Id);
                    var identity = await userManager.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie);
                    authManager.SignIn(new AuthenticationProperties { IsPersistent = false }, identity);

                    if (roles.Contains(UserRoleNames.Agent))
                    {
                        await _agentRepository.UpdateLastSeenDateAsync(agentId, DateTime.Now);
                    }
                    if (roles.Contains(UserRoleNames.Manager))
                    {
                        try
                        {
                            var result = await _t1000Logic.Login(user.UserName, model.Password);
                            await userManager.AddOrReplaceClaimAsync(user.Id, Common.Constants.DbsidClaimType, result.Dbsid);
                            await userManager.AddOrReplaceClaimAsync(user.Id, Common.Constants.ClientIdClaimType, result.UserId);
                        }
                        catch (Exception ex)
                        {
                            _logger.Error("Ошибка авторизации в Т1000", ex);
                        }
                    }

                    if (!string.IsNullOrEmpty((model.ReturnUrl ?? "").Trim(" /".ToCharArray())) && Url.IsLocalUrl(model.ReturnUrl))
                    {
                        return model.ReturnUrl;
                    }
                    return Url.Action("tasks", _defaultControllerName);
                }
                else
                {
                    throw new PublicException("Неверные учетные данные");
                }
            }, "Ошибка при попытке авторизации");
        }

        [HttpGet]
        public async Task<ActionResult> Logout()
        {
            try
            {
                var userManager = HttpContext.GetOwinContext().GetUserManager<BkUserManager>();
                var userId = User.Identity.GetUserId();
                var dbsidClaim = (await userManager.GetClaimsAsync(userId)).FirstOrDefault(p => p.Type == Common.Constants.DbsidClaimType);
                if (dbsidClaim?.Value !=  null)
                {
                    await _t1000Logic.Logout(dbsidClaim.Value);
                    await userManager.RemoveClaimAsync(userId, dbsidClaim);
                }
                else
                {
                    _logger.Error($"dbsid пользователя {userId} не найден");
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Ошибка аутентификации через сервис", ex);
            }

            HttpContext.GetOwinContext().Authentication.SignOut(DefaultAuthenticationTypes.ApplicationCookie);

            return View("Login");
        }

        [BkAuthorize(Roles = UserRoleNames.Manager)]
        [HttpGet]
        public async Task<JsonResult> RemoveUser(string username)//TODO: remove
        {
            var r = (await RunWithResult(async () =>
            {
                var userManager = HttpContext.GetOwinContext().GetUserManager<BkUserManager>();

                var user = await userManager.FindByNameAsync(username);
                if (user != null)
                {
                    var result = await userManager.DeleteAsync(user);
                    if (!result.Succeeded)
                    {
                        throw new Exception(string.Join(". ", result.Errors));
                    }

                    return "Пользователь удален";
                }

                return "Пользователь не существует";
            }, "При удалении пользователя произошла ошибка"));

            r.JsonRequestBehavior = JsonRequestBehavior.AllowGet;

            return r;
        }

        [HttpGet]
        public async Task<JsonResult> Register(string username, string password, string roleName)//TODO: remove
        {
            var r = (await RunWithResult(async () =>
            {
                var userManager = HttpContext.GetOwinContext().GetUserManager<BkUserManager>();
                var roleManager = HttpContext.GetOwinContext().GetUserManager<BkRoleManager>();

                var existingUser = await userManager.FindByNameAsync(username);
                if (existingUser != null)
                {
                    throw new PublicException($"Пользователь с именем {username} уже существует");
                }

                if (!roleManager.RoleExists(roleName))
                {
                    var roleCreationResult = await roleManager.CreateAsync(new BkRole(roleName));
                    if (!roleCreationResult.Succeeded)
                    {
                        throw new PublicException(string.Join(". ", roleCreationResult.Errors));
                    }
                }

                var user = new BkUser { UserName = username };
                var userCreationResult = await userManager.CreateAsync(user, password);
                if (!userCreationResult.Succeeded)
                {
                    throw new PublicException(string.Join(". ", userCreationResult.Errors));
                }

                var addToRoleResult = await userManager.AddToRoleAsync(user.Id, roleName);
                if(!addToRoleResult.Succeeded)
                {
                    throw new PublicException(string.Join(". ", addToRoleResult.Errors));
                }

                return user.Id;
            }, "При создании пользователя произошла ошибка"));

            r.JsonRequestBehavior = JsonRequestBehavior.AllowGet;

            return r;
        }
    }
}