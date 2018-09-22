using Microsoft.AspNet.Identity.EntityFramework;

namespace Bookkeeping.Data.Models
{
    public class BkRole : IdentityRole
    {
        public BkRole() : base()
        {
        }

        public BkRole(string name)
            : base(name)
        {
        }
    }
}
