using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bookkeeping.Data.Models
{
    [Table("Resolutions", Schema = "public")]
    public class Resolution
    {
        public int Id { get; set; }
        public string Note { get; set; }
        public bool? Decision { get; set; }
        public string PurposeOfPaymentComment { get; set; }
        public string ImageData { get; set; }
        public string ImageName { get; set; }
        public string MimeType { get; set; }
        public DateTime? DateResolution { get; set; }
        public int AgentId { get; set; }
        public Agent Agent { get; set; }
        public int TaskId { get; set; }
        public Task Task { get; set; }
        public DateTime DateTimeDesktopUpdate { get; set; }
        public DateTime DateTimeMobileUpdate { get; set; }
    }
}