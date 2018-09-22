using System.Collections.Generic;

namespace Bookkeeping.BusinessLogic
{
    public class UserModel<T> where T : class
    {
        public List<T> User { get; set; }
    }
}
