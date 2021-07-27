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
using System.Data.SqlClient;

namespace RedisCacheExample.Controllers
{
    public class CampaignsController : Controller
    {
        public async Task<ActionResult> Index(string Sessionid)
        {
            //Create a connection to Redis
            using(ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("<Redis Cache Connection String>"))
            {
                //Get Redis Database
                var db = redis.GetDatabase();

                //set viewmodel so they it is not null for our view
                CampaignsModel campaigns = new CampaignsModel()
                {
                  CampaignTypes = new List<CampaignTypes>(),
                  Session = new SessionModel()  
                };

                //Check is session id exists in redis
                if(await db.KeyExistsAsync(Sessionid))
                {                   
                    //session id exists in Redis check for campaign cache in redis
                    if(await db.KeyExistsAsync("CampaignTypes"))
                    {
                        //campaigns are cached, get data from redis
                        var campaignCache  = await db.StringGetAsync("CampaignTypes");
                        campaigns.CampaignTypes = JsonConvert.DeserializeObject<List<CampaignTypes>>(campaignCache);
                    }
                    else
                    {
                        //campaigns are not cached, get campaigns from SQL
                        campaigns.CampaignTypes = await GetCampaigns();

                        //save campaigns to Redis for future use
                        await db.StringSetAsync("CampaignTypes", JsonConvert.SerializeObject(campaigns.CampaignTypes), TimeSpan.FromMinutes(5));
                    }


                    //pull session information from Redis
                    var session = await db.StringGetAsync(Sessionid);
                    campaigns.Session = JsonConvert.DeserializeObject<SessionModel>(session);
                    campaigns.Session.Id = Sessionid;

                    //A refresh of the page should extend our session open by 10 minutes
                    await db.KeyExpireAsync(Sessionid, TimeSpan.FromMinutes(10));

                    return View(campaigns);
                }
                else{
                    //session expired
                    return RedirectToAction("Index", "Home");
                }
            }
        }

        async Task<List<CampaignTypes>> GetCampaigns()
        {
            //Replace with your sql connection string
            string cs = "<Sql Server Connection String>";

            //List to hold our campaigns
            List<CampaignTypes> types = new List<CampaignTypes>();

            //connct to sql
            using(SqlConnection connection = new SqlConnection(cs))
            {
                //Open our SQL connection
                connection.Open();

                //complex SQL query worthy of being cached
                using(SqlCommand cmd = new SqlCommand(@"select Count(dbo.campaign_tracking.Campaign) EmailsOpened, dbo.campaigns.CampaignId, dbo.campaigns.Subject from dbo.campaign_tracking
                                                        right join  dbo.campaigns on dbo.campaign_tracking.Campaign = dbo.campaigns.CampaignId
                                                        Group By  dbo.campaign_tracking.Campaign, dbo.campaigns.CampaignId, dbo.campaigns.Subject", connection))
                {

                    //execute query
                    SqlDataReader reader = await cmd.ExecuteReaderAsync();
                    while(await reader.ReadAsync())
                    {
                        //object for storing campaign information
                        CampaignTypes type = new CampaignTypes()
                        {
                            CampaignId = reader["CampaignId"]!= null ? reader["CampaignId"].ToString() : "invalid id",
                            Subject = reader["Subject"] != null ? reader["Subject"].ToString() : "Subject not found",
                            EmailCount = reader["EmailsOpened"] != null ? Convert.ToInt32(reader["EmailsOpened"]) : 0
                        };

                        types.Add(type);
                    }
                }

                //close connection
                connection.Close();
            }

            //return our list of campaigns
            return types;
        }
    }
}