using Bookkeeping.Common.Exceptions;
using Bookkeeping.Common.Interfaces;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Bookkeeping.WebUi.Controllers
{
    public class BaseController : Controller
    {
        protected readonly ILogger _logger;

        public BaseController(ILogger logger)
        {
            _logger = logger;
        }

        protected async Task<JsonResult> RunWithResult(Func<Task<object>> func, string errorMessage)
        {
            try
            {
                return new JsonResult
                {
                    Data = new
                    {
                        data = await func()
                    },
                    MaxJsonLength = Int32.MaxValue
                }; 
            }
            catch(SessionExpiredException ex)
            {
                _logger.Error("Время сессии истекло", ex);
                return new JsonResult
                {
                    Data = new
                    {
                        error = ex.Message,
                        redirectUrl = Url.Action("logout", "account")
                    }
                };
            }
            catch (PublicException ex)
            {
                _logger.Error(errorMessage, ex);
                return new JsonResult
                {
                    Data = new
                    {
                        error = ex.Message
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.Error(errorMessage, ex);
                return new JsonResult
                {
                    Data = new
                    {
                        error = errorMessage
                    }
                };
            }
        }

        protected async Task<JsonResult> Run(Func<Task> func, string errorMessage)
        {
            try
            {
                await func();
                return new JsonResult
                {
                    Data = new
                    {
                        data = string.Empty
                    }
                };
            }
            catch (SessionExpiredException ex)
            {
                _logger.Error("Время сессии истекло", ex);
                return new JsonResult
                {
                    Data = new
                    {
                        error = ex.Message,
                        redirectUrl = Url.Action("logout", "account")
                    }
                };
            }
            catch (PublicException ex)
            {
                _logger.Error(errorMessage, ex);
                return new JsonResult
                {
                    Data = new
                    {
                        error = ex.Message
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.Error(errorMessage, ex);
                return new JsonResult
                {
                    Data = new
                    {
                        error = errorMessage
                    }
                };
            }
        }

        protected string RenderViewToString(string viewName, object model)
        {
            ViewData.Model = model;
            using (var sw = new StringWriter())
            {
                var viewResult = ViewEngines.Engines.FindPartialView(ControllerContext,
                                                                         viewName);
                var viewContext = new ViewContext(ControllerContext, viewResult.View,
                                             ViewData, TempData, sw);
                viewResult.View.Render(viewContext, sw);
                viewResult.ViewEngine.ReleaseView(ControllerContext, viewResult.View);
                return sw.GetStringBuilder().ToString();
            }
        }
    }
}