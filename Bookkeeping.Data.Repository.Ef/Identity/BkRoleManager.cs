using Bookkeeping.Data.Context;
using Bookkeeping.Data.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;

namespace Bookkeeping.Data.Repository.Ef
{
    public class BkRoleManager : RoleManager<BkRole>
    {
        public BkRoleManager(RoleStore<BkRole> store)
            : base(store)
        {
        }

        public static BkRoleManager Create(IdentityFactoryOptions<BkRoleManager> options, IOwinContext context)
        {
            var manager = new BkRoleManager(new RoleStore<BkRole>(context.Get<BkDbContext>()));
            return manager;
        }
    }
}
