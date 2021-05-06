using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OfficeEmployeeDirectory.Models;
using Microsoft.AspNetCore.Authorization;
using System.Net.Http;
using Microsoft.Graph;
using Microsoft.Graph.Core;
using Microsoft.Identity.Client;
using Microsoft.Extensions.Configuration;

namespace OfficeEmployeeDirectory.Controllers
{
    //authorized controller
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        //get access to config
        private IConfiguration configuration;

        public HomeController(ILogger<HomeController> logger, IConfiguration Configuration)
        {
            _logger = logger;
            configuration = Configuration;
        }

        //home page is anonymous so it can load in teams
        [AllowAnonymous]
        public IActionResult Index()
        {
            ViewBag.SignInUrl = configuration.GetValue<string>("SignInUrl");
           
            return View();
        }

        
        //default from project
        public IActionResult Privacy()
        {
            return View();
        }

        //default from project
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


        //our secure endpoint
        [HttpGet]
        public async Task<IActionResult> GetDirectory()
        {
            try
            {
                //pulls config data for our graph api
                string tenantId = configuration.GetSection("DirectoryApp").GetValue<string>("TenantId"); //realm
                //some service account with graph api permissions
                string clientId = configuration.GetSection("DirectoryApp").GetValue<string>("ClientId"); 
                //service account password
                string clientSecret = configuration.GetSection("DirectoryApp").GetValue<string>("ClientSecret");; 
                string[] scopes = new string[] {"https://graph.microsoft.com/.default" };

                //creates a header for accessing our graph api
                IConfidentialClientApplication app = ConfidentialClientApplicationBuilder.Create(clientId)
                        .WithClientSecret(clientSecret)
                        .WithAuthority(new Uri("https://login.microsoftonline.com/" + tenantId))
                        .Build();

                //gets token
                AuthenticationResult result = await app.AcquireTokenForClient(scopes).ExecuteAsync();

                //creates graph client
                GraphServiceClient client = new GraphServiceClient("https://graph.microsoft.com/v1.0", new DelegateAuthenticationProvider(
                async (requestMessage) =>
                {
                    //adds token to header
                    requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", result.AccessToken);
                }
                ));


                //pulls all users who's account is enabled
                var users = await client.Users.Request().Filter("AccountEnabled eq true").GetAsync();

                return Json(users);

            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
