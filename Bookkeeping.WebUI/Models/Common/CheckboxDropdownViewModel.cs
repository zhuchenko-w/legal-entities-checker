using System.Collections.Generic;

namespace Bookkeeping.WebUi.Models
{
    public class CheckboxDropdownViewModel
    {
        public string Id { get; set; }
        public string AllText { get; set; }
        public string NoneText { get; set; }
        public string ShortLabelText { get; set; }
        public bool AlwaysSet { get; set; }
        public bool IsRequired { get; set; }
        public List<KeyValuePair<int, string>> Items { get; set; }
    }
}