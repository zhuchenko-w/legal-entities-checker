using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Bookkeeping.BusinessLogic
{
    public class Helper
    {
        public static async Task<string> Post(HttpContent data, string uri)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                try
                {
                    var response = await client.PostAsync(uri, data);
                    return response.IsSuccessStatusCode ? response.Content.ReadAsStringAsync().Result : null;
                }
                catch (Exception ex)
                {
                    throw new Exception("Post request error", ex);
                }
            }
        }
    }
}
