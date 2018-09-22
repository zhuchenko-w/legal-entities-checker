using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bookkeeping.Data.Models
{
    [NotMapped]
    public class AgentResolutionModel
    {
        public int Id { get; set; }
        public string Note { get; set; }
        public bool? Decision { get; set; }
        public string PurposeOfPaymentComment { get; set; }
        public DateTime DateTimeDesktopUpdate { get; set; }
        public string ImageData { get; set; }
        public string ImageName { get; set; }
        public string MimeType { get; set; }
        public int AgentId { get; set; }
        public int TaskId { get; set; }
        public string Inn { get; set; }
        public string PurposeOfPayment { get; set; }
    }
}
