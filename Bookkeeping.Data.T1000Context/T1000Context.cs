using System.Data.Entity;

namespace Bookkeeping.Data.Context
{
    public class T1000Context : DbContext
    {
        public T1000Context() : base("T1000Context")
        {
        }

        public static T1000Context Create()
        {
            return new T1000Context();
        }
    }
}
