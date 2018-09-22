using Bookkeeping.Data.Context;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using BkUser = Bookkeeping.Data.Models.BkUser;

namespace Bookkeeping.Data.Repository.Ef
{
    public class BkUserManager : UserManager<BkUser>
    {
        public BkUserManager(IUserStore<BkUser> store) 
            : base(store)
        {
        }

        public static BkUserManager Create(IdentityFactoryOptions<BkUserManager> options, IOwinContext context)
        {
            var manager = new BkUserManager(new UserStore<BkUser>(context.Get<BkDbContext>()))
            {
                PasswordValidator = new PasswordValidator
                {
                    RequiredLength = Common.Constants.MinPasswordLength
                }
            };

            return manager;
        }

        public async Task AddOrReplaceClaimAsync(string userId, string claimType, string value)
        {
            var claim = (await GetClaimsAsync(userId)).FirstOrDefault(p => p.Type == claimType);
            if (claim != null)
            {
                await RemoveClaimAsync(userId, claim);
            }
            await AddClaimAsync(userId, new Claim(claimType, value));
        }
    }
}
