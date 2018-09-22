using System.Web;

namespace Bookkeeping.WebUi.Models
{
    public class AgentResolutionViewModel
    {
        public int Id { get; set; }
        public string Note { get; set; }
        public bool? Decision { get; set; }
        public string PurposeOfPaymentComment { get; set; }
        public HttpPostedFileWrapper ImageFile { get; set; }
        public string ImageData { get; set; }
        public string MimeType { get; set; }
        public string ImageName { get; set; }
        public int TaskId { get; set; }
        public string Inn { get; set; }
        public string PurposeOfPayment { get; set; }
    }
}