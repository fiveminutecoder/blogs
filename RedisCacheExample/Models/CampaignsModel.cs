using System.Collections.Generic;

namespace RedisCacheExample.Models
{
    public class CampaignsModel
    {
        public SessionModel Session {get;set;}
        public List<CampaignTypes> CampaignTypes {get;set;}
        
    }
}