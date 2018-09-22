using System.Collections.Generic;

namespace Bookkeeping.BusinessLogic
{
    public class EntryModel<T> where T : class
    {
        public List<T> Entry { get; set; }
    }
}
