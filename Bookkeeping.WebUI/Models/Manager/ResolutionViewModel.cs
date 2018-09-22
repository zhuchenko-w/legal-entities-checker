using Bookkeeping.Data.Models;
using System;

namespace Bookkeeping.WebUi.Models
{
    public class ResolutionViewModel
    {
        public int Id { get; set; }
        public string Note { get; set; }
        public bool? Decision { get; set; }
        public string PurposeOfPaymentComment { get; set; }
        public string ImageData { get; set; }
        public string ImageName { get; set; }
        public string MimeType { get; set; }
        public DateTime? DateResolution { get; set; }
        public Agent Agent { get; set; }
        public DateTime DateTimeDesktopUpdate { get; set; }
        public DateTime DateTimeMobileUpdate { get; set; }
    }
}