﻿using System.Net;
using System.Web.Mvc;

namespace Bookkeeping.WebUi.Controllers
{
    public class ErrorController : Controller
    {
        public ActionResult Index()
        {
            Response.TrySkipIisCustomErrors = true;
            Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            return View();
        }
        public ActionResult InvalidParameter()
        {
            Response.TrySkipIisCustomErrors = true;
            Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return View();
        }
        public ActionResult NotFound()
        {
            Response.TrySkipIisCustomErrors = true;
            Response.StatusCode = (int)HttpStatusCode.NotFound;
            return View();
        }
        public ActionResult Forbidden()
        {
            Response.TrySkipIisCustomErrors = true;
            Response.StatusCode = (int)HttpStatusCode.Forbidden;
            return View();
        }
    }
}