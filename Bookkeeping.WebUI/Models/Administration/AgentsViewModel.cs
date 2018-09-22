using Bookkeeping.Data.Models;
using System.Collections.Generic;

namespace Bookkeeping.WebUi.Models
{
    public class AgentsViewModel
    {
        public IList<Agent> Agents { get; set; }
        public string AgentsError { get; set; }
    }
}