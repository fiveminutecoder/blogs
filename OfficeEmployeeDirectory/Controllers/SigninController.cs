using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OfficeEmployeeDirectory.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;

namespace OfficeEmployeeDirectory.Controllers
{
    //just controls the login views routing
    [AllowAnonymous]
    public class SigninController: Controller
    {
        private IConfiguration configuration;
        public SigninController(IConfiguration Configuration)
        {
            configuration = Configuration;
        }

        public ActionResult SigninStart()
        {
            ViewBag.ReturnUrl = configuration.GetValue<string>("ReturnUrl");
            ViewBag.ClientId = configuration.GetSection("AzureAd").GetValue<string>("Audience");
            return View();
        }

        public ActionResult SigninEnd()
        {
            return View();
        }
    }
}