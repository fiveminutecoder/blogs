using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RedisCacheExample.Models;
using StackExchange.Redis;
using Newtonsoft.Json;
    

namespace RedisCacheExample.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(SessionModel Model)
        {
            //creates a unique session id
            Guid sessionId = Guid.NewGuid();

            //This is a 5 minute project so we are going to code in controller
            //Create connection to Redis
            using(ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("<Redis Cache Connection String>"))
            {

                //Get database, this returns default database
                var db = redis.GetDatabase();

                //add session information to Redis with a 10 minute expiration time
                await db.StringSetAsync(sessionId.ToString(), JsonConvert.SerializeObject(Model),TimeSpan.FromMinutes(10));

            }

            //Session created, now go to campaigns
            return RedirectToAction("Index", "Campaigns", new { SessionId=sessionId.ToString()});
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
