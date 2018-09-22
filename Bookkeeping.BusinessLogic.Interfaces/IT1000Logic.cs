using Bookkeeping.BusinessLogic.Models;
using System.Threading.Tasks;
using SearchByInnResult = Bookkeeping.Data.Models.SearchByInnResult;

namespace Bookkeeping.BusinessLogic.Interfaces
{
    public interface IT1000Logic
    {
        Task<LoginResult> Login(string login, string password);
        Task Logout(string dbsid);
        Task<SearchByInnResult> GetCompanyNameByInn(string inn, string dbSid);
    }
}
