using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThunderCLI.Utils
{
    public class JsonUtils
    {
        public static async Task<JObject> GetResponseAsJObjectAsync(string jsonUrl)
        {
            HttpClient client = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };
            HttpResponseMessage response = await client.GetAsync(jsonUrl);
            string responseRaw = await response.Content.ReadAsStringAsync();
            JObject deserial = (JObject)JsonConvert.DeserializeObject(responseRaw);
            return deserial;
        }
    }
}
