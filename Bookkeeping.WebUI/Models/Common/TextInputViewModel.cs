namespace Bookkeeping.WebUi.Models
{
    public class TextInputViewModel
    {
        public string Id { get; set; }
        public string Value { get; set; }
        public string FullLabelText { get; set; }
        public string ShortLabelText { get; set; }
        public string InputFieldClassString { get; set; }
        public bool IsRequired { get; set; }
        public int MaxLength { get; set; }
    }
}