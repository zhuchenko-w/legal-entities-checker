
namespace Bookkeeping.Models
{
    public class CreateTaskViewModel
    {
        public string Inn { get; set; }
        public string Name { get; set; }
        public string PurposeOfPayment { get; set; }
        public int[] AgentIds { get; set; }
    }
}