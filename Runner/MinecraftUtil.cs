using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Runner
{
    public class MinecraftUtil
    {

        public static readonly string VERSION_URL = "https://launchermeta.mojang.com/mc/game/version_manifest.json";

        public enum MinecraftVersionType
        {
            Release,
            Snapshot,
            Both
        }

        public static async Task<string> GetLatestVersionAsync(MinecraftVersionType type)
        {
            JObject response = await GetResponseAsJson(VERSION_URL);
            if (type == MinecraftVersionType.Both)
            {
                return response["latest"].ToString();
            }
            return response["latest"][type.ToString().ToLower()].ToString();
        }

        public static async Task<List<string>> GetAllVersionsAsync(MinecraftVersionType type)
        {
            JObject response = await GetResponseAsJson(VERSION_URL);
            JArray versions = (JArray)response["versions"];

            List<string> versionsList;
            if (type == MinecraftVersionType.Both) versionsList = versions.Select(c => c["id"].ToString()).ToList();
            else versionsList = versions.Where(c => c["type"].ToString() == type.ToString().ToLower()).Select(c => c["id"].ToString()).ToList();

            return versionsList;
        }

        private static async Task<JObject> GetResponseAsJson(string url)
        {
            HttpClient client = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
            HttpResponseMessage response = await client.GetAsync(url);
            string responseRaw = await response.Content.ReadAsStringAsync();
            JObject deserial = (JObject)JsonConvert.DeserializeObject(responseRaw);
            return deserial;
        }
    }
}
