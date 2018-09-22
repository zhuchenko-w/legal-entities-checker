using Newtonsoft.Json;

namespace Bookkeeping.BusinessLogic.Models
{
    public class Company
    {
        [JsonProperty("nameopf")]
        public string NameCompany { get; set; }
    }
}
