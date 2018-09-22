using System.Threading.Tasks;
using LoginResultDomain = Bookkeeping.Data.Models.LoginResultDomain;
using SearchByInnResult = Bookkeeping.Data.Models.SearchByInnResult;

namespace Bookkeeping.Data.Repository.Interfaces
{
    public interface IT1000Repository
    {
        Task<LoginResultDomain> Login(string login, string password, string userarm);
        Task Logout(string dbsid);
        Task<SearchByInnResult> SearchCompanyNameByInnAsync(string inn, string dbsid);
    }
}