using Bookkeeping.Common.Exceptions;
using Bookkeeping.Data.Context;
using Bookkeeping.Data.Repository.Interfaces;
using System;
using System.Threading.Tasks;
using LoginResultDomain = Bookkeeping.Data.Models.LoginResultDomain;
using SearchByInnResult = Bookkeeping.Data.Models.SearchByInnResult;

namespace Bookkeeping.Data.Repository.Ef
{
    public class T1000Repository : IT1000Repository
    {
        private const int ResumeSessionMaxTries = 3;

        public async Task<LoginResultDomain> Login(string login, string password, string userarm)
        {
            using (var db = new T1000Context())
            {
                return await db.Database.SqlQuery<LoginResultDomain>($"SELECT dbsId, userId FROM armulcheck.sp_login_wso('{login}','{password}','{userarm}') LIMIT 1").FirstOrDefaultAsync();
            }
        }

        public async Task Logout(string dbsid)
        {
            using (var db = new T1000Context())
            {
                await db.Database.ExecuteSqlCommandAsync($"SELECT armulcheck.sp_logout('{dbsid}')");
            }
        }

        public async Task<SearchByInnResult> SearchCompanyNameByInnAsync(string inn, string dbsid)
        {
            using (var db = new T1000Context())
            {
                return await SearchCompanyNameByInn(db, inn, dbsid, ResumeSessionMaxTries);
            }
        }

        private async Task<SearchByInnResult> SearchCompanyNameByInn(T1000Context db, string inn, string dbsid, int resumeSessionTriesLeft)
        {
            var newDbsid = dbsid;

            var result = await db.Database.SqlQuery<string>($"SELECT armulcheck.proc_spr_ul_check_inn_wso('{dbsid}','{inn}')").FirstOrDefaultAsync() ?? "";

            switch (result)
            {
                case "-1":
                case "-1 Сессия не существует или не активна.":
                    throw new PublicException("Пользователь не авторизован в T1000");
                case "-2":
                case "-2 Время сеанса истекло":
                    if(resumeSessionTriesLeft > 0)
                    {
                        newDbsid = await ResumeSession(db, dbsid);
                        return await SearchCompanyNameByInn(db, inn, dbsid, resumeSessionTriesLeft - 1);
                    }
                    throw new SessionExpiredException("Не удалось продлить сессию");
                case "-3":
                    throw new PublicException("Нет прав доступа");
                default:
                    return new SearchByInnResult {
                        CompanyName = result,
                        NewDbsid = newDbsid
                    };
            }
        }

        private async Task<string> ResumeSession(T1000Context db, string dbsid)
        {
            try
            {
                var result = await db.Database.SqlQuery<string>($"SELECT armulcheck.db_session_resume('{dbsid}')").FirstOrDefaultAsync();

                switch (result)
                {
                    case "-1":
                    case "-1 Сессия не существует или не активна.":
                        throw new PublicException("Пользователь не авторизован в T1000");
                    case "-2":
                    case "-2 Время сеанса истекло":
                        throw new SessionExpiredException("Время сессии истекло");
                    case "-3":
                        throw new PublicException("Нет прав доступа");
                    default:
                        return result;
                }
            }
            catch (SessionExpiredException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new PublicException("Ошибка продления сессии", ex);
            }
        }
    }
}
