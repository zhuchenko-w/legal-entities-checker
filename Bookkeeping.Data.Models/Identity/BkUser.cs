using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;

namespace Bookkeeping.Data.Models
{
    public class BkUser : IdentityUser
    {
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<BkUser> manager)
        {
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);

            var dbsidClaim = Claims.FirstOrDefault(c => c.ClaimType == Common.Constants.DbsidClaimType);
            if (dbsidClaim != null)
            {
                userIdentity.AddClaim(new Claim(dbsidClaim.ClaimType, dbsidClaim.ClaimValue));
            }

            return userIdentity;
        }
    }
}
