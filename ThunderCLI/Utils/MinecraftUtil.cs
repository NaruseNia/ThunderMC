using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using static ThunderCLI.Utils.JsonUtils;

namespace ThunderCLI.Utils
{
    public class MinecraftUtil
    {

        public const string MINECRAFT_VERSION_MANIFEST_URL = "https://launchermeta.mojang.com/mc/game/version_manifest.json";
        public const string FORGE_MAVEN_VERSION_MANIFEST_URL = "https://files.minecraftforge.net/net/minecraftforge/forge/maven-metadata.json";
        public const string FORGE_MAVEN_VERSION_SLIM_MANIFEST_URL = "https://files.minecraftforge.net/net/minecraftforge/forge/promotions_slim.json";
        public const string FABRIC_API_URL = "https://meta.fabricmc.net/";
        public const string FABRIC_API_ALL_VERSION_URL = "https://meta.fabricmc.net/v2/versions";
        public const string SPIGOT_BUILDTOOLS_DOWNLOAD_URL = "https://hub.spigotmc.org/jenkins/job/BuildTools/lastSuccessfulBuild/artifact/target/BuildTools.jar.";

        public enum MinecraftVersionType
        {
            Release,
            Snapshot,
            Both
        }

        public class MinecraftVersion
        {
            public short MajorVersion { get; set; }
            public short MinorVersion { get; set; }
            public short RevisionVersion { get; set; }
            public bool isPreview { get; set; }
        }

        public static MinecraftVersion? ConvertMinecraftVersionFromString(string versionStr)
        {
            Regex versionFormat = new(@"[1-9]\.[0-9]+\.?[1-9]*");
            Regex versionFormatPre = new(@"[1-9]\.[0-9]+-pre[1-9]*");
            if (versionFormat.IsMatch(versionStr))
            {
                string[] versions = versionStr.Split(".");
                return new MinecraftVersion()
                {
                    MajorVersion = short.Parse(versions[0]),
                    MinorVersion = short.Parse(versions[1]),
                    RevisionVersion = short.Parse(versions[2]),
                    isPreview = false
                };
            }
            else if (versionFormatPre.IsMatch(versionStr))
            {
                string[] versions = versionStr.Split(".");
                return new MinecraftVersion()
                {
                    MajorVersion = short.Parse(versions[0]),
                    MinorVersion = short.Parse(versions[1].Split("-")[0]),
                    RevisionVersion = 0,
                    isPreview = true
                };
            }
            return null;
        }

        public static string GetForgeDownloadURLFromVersion(string forgeVersion)
        {
            return $@"https://maven.minecraftforge.net/net/minecraftforge/forge/{forgeVersion}/forge-{forgeVersion}-installer.jar";
        }

        public static async Task<string> GetServerDownloadURL(string version)
        {
            List<string> k = await GetAllVersionsAsync(MinecraftVersionType.Both);
            if (!k.Contains(version)) throw new InvalidDataException("Didn't found that version.");
            JObject response = await GetResponseAsJObjectAsync(MINECRAFT_VERSION_MANIFEST_URL);
            JArray versions = (JArray)response["versions"];

            string manifestUrl = versions.Where(c => c["id"].ToString() == version).Select(c => c["url"].ToString()).ToList().First();

            JObject manifestJson = await GetResponseAsJObjectAsync(manifestUrl);

            string res = manifestJson["downloads"]["server"]["url"].ToString();

            return res;
        }

        public static async Task<string> GetLatestVersionAsync(MinecraftVersionType type)
        {
            JObject response = await GetResponseAsJObjectAsync(MINECRAFT_VERSION_MANIFEST_URL);
            if (type == MinecraftVersionType.Both)
            {
                return response["latest"].ToString();
            }
            return response["latest"][type.ToString().ToLower()].ToString();
        }

        public async static Task<List<string>> GetAllVersionsAsync(MinecraftVersionType type)
        {
            JObject response = await GetResponseAsJObjectAsync(MINECRAFT_VERSION_MANIFEST_URL);
            JArray versions = (JArray)response["versions"];

            List<string> versionsList;
            if (type == MinecraftVersionType.Both) versionsList = versions.Select(c => c["id"].ToString()).ToList();
            else versionsList = versions.Where(c => c["type"].ToString() == type.ToString().ToLower()).Select(c => c["id"].ToString()).ToList();

            return versionsList;
        }


    }
}
