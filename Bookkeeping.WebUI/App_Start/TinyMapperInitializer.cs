using Bookkeeping.BusinessLogic.Models;
using Bookkeeping.Data.Models;
using Nelibur.ObjectMapper;

namespace Bookkeeping.WebUi.App_Start
{
    public static class TinyMapperInitializer
    {
        public static void Initialize()
        {
            TinyMapper.Bind<LoginResultDomain, LoginResult>(config =>
            {
                config.Bind(source => source.dbsId, target => target.Dbsid);
                config.Bind(source => source.userId, target => target.UserId);
            });
        }
    }
}