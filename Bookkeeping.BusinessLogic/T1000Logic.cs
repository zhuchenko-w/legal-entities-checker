using Bookkeeping.BusinessLogic.Interfaces;
using Bookkeeping.BusinessLogic.Models;
using Bookkeeping.Common.Interfaces;
using Bookkeeping.Data.Repository.Interfaces;
using Nelibur.ObjectMapper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using SearchByInnResult = Bookkeeping.Data.Models.SearchByInnResult;

namespace Bookkeeping.BusinessLogic
{
    public class T1000Logic : IT1000Logic
    {
        private readonly IT1000Repository _t1000Repository;

        private readonly string _loginUrl;
        private readonly string _logoutUrl;
        private readonly string _getNameByInnUrl;
        private readonly string _userarm;

        public T1000Logic(ISettingsManager settingsManager, IT1000Repository t1000Repository)
        {
            _t1000Repository = t1000Repository;
            _loginUrl = settingsManager.GetValue<string>("LoginUrl");
            _logoutUrl = settingsManager.GetValue<string>("LogoutUrl");
            _userarm = settingsManager.GetValue<string>("userarm");
            _getNameByInnUrl = settingsManager.GetValue<string>("GetNameByInnUrl");
        }

        public async Task<LoginResult> Login(string login, string password)
        {
            var result = await _t1000Repository.Login(login, password, _userarm);
            return TinyMapper.Map<LoginResult>(result);
            //return await LoginUsingService(login, password);
        }

        public async Task Logout(string dbsid)
        {
            await _t1000Repository.Logout(dbsid);
            //await LogoutUsingService(dbsid);
        }

        public async Task<SearchByInnResult> GetCompanyNameByInn(string inn, string dbsid)
        {
            return await _t1000Repository.SearchCompanyNameByInnAsync(inn, dbsid);
            //return new SearchByInnResult {
            //    CompanyName = await GetCompanyNameByInnUsingService(inn, dbsid),
            //    NewDbsid = dbsid 
            //};
        }

        private async Task<LoginResult> LoginUsingService(string login, string password)
        {
            var data = new Dictionary<string, string>
            {
                { "userlogin", login },
                { "userpass", password },
                { "userarm", _userarm }
            };

            try
            {
                var content = await Helper.Post(new FormUrlEncodedContent(data), _loginUrl);
                if (content == null)
                    throw new Exception("Ответ сервера не содержит данных");

                var jObj = JObject.Parse(content);
                var entry = jObj.SelectToken("Entries.Entry[0]");
                return entry.ToObject<LoginResult>();
            }
            catch (JsonReaderException ex)
            {
                throw new Exception($"Ошибка парсинга ответа сервиса {_loginUrl}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка входа через сервис {_loginUrl}", ex);
            }
        }

        private async Task LogoutUsingService(string dbsid)
        {
            var data = new Dictionary<string, string> { { "dbsid", dbsid } };

            try
            {
                await Helper.Post(new FormUrlEncodedContent(data), _logoutUrl);
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка выхода через сервис {_loginUrl}", ex);
            }
        }

        private async Task<string> GetCompanyNameByInnUsingService(string inn, string dbSid)
        {
            var data = new Dictionary<string, string>
            {
                { "dbsid", dbSid },
                { "inn", inn }
            };
            var url = _getNameByInnUrl;
            var content = await Helper.Post(new FormUrlEncodedContent(data), url);
            if (content != null)
            {
                try
                {
                    var result = JsonConvert.DeserializeObject<Response<Company>>(content);
                    var company = result.Entries.Entry[0].NameCompany;
                    return company;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            return string.Empty;
        }
    }
}
