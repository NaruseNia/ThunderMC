using Logger;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Net;
using System.Net.Mime;
using System.Xml.Linq;
using System.Xml.Serialization;

using static ThunderCLI.Utils.MinecraftUtil;
using static ThunderCLI.Utils.JsonUtils;
using System.Text;

public class Thunder
{
    private static readonly LogBuilder log = new();
    private static string MCServerFilePath { get; set; } = "";
    private static string InstanceFolderDefault { get; set; } = @"C:\Users\notsh\projects\ThunderTest\Instances";

    public enum ServerType
    {
        Vanilla,
        Forge,
        Fabric,
        Spigot,
        Paper,
        Sponge,
        Magma
    }

    private enum Mode
    {
        Make,
        Load,
        Convert
    }

    public class DownloadFileInfo
    {
        public string Url { get; set; }
        public string Name { get; set; }
        public DownloadFileInfo(string url, string name)
        {
            Url = url;
            Name = name;
        }
    }

    public struct MCServer
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public ServerType type { get; set; }
        public string JarName { get; set; }
    }

    public async static Task Test()
    {
        Console.WriteLine(await GetServerDownloadURL("1.14.2"));
        Console.WriteLine(Environment.GetEnvironmentVariable("JAVA_HOME"));
    }

    public async static Task<int> Run(string[] args)
    {
        log.Init().Add("Select Mode").Newline()
                          .Add("(1) Make").Newline()
                          .Add("(2) Load").Newline().Put();
        _ = int.TryParse(Console.ReadLine(), out int modeIn);
        var mode = (Mode)modeIn - 1;
        Console.WriteLine(mode);

        switch (mode)
        {
            case Mode.Load:
                #region Load
                if (args.Length == 0)
                {
                    log.Init().Add("Please input .MCServer file path.").Newline().Put();
                    MCServerFilePath = /* Console.ReadLine() ?? */ @"C:\Users\notsh\projects\ThunderTest\TestServer\TestServer.MCServer";
                }
                else
                {
                    MCServerFilePath = args[0];
                }

                Console.WriteLine(MCServerFilePath);

                if (!File.Exists(MCServerFilePath))
                {
                    log.Init().Add("Error: File not found:" + MCServerFilePath).Newline().Put();
                    return -1;
                }

                XmlSerializer serializer = new(typeof(MCServer));
                object? serializedMCServer; MCServer mcserver;
                using (StreamReader reader = new(MCServerFilePath))
                {
                    serializedMCServer = serializer.Deserialize(reader);
#pragma warning disable CS8605
                    mcserver = (MCServer)serializedMCServer;
#pragma warning restore CS8605
                }

                Console.WriteLine(mcserver.Name);
                break;
            #endregion Load
            case Mode.Make:

                MCServer serverInfo = new();

                if (!Directory.Exists(InstanceFolderDefault)) Directory.CreateDirectory(InstanceFolderDefault);
                Console.WriteLine("Input Instance name");

                string? instanceName = Console.ReadLine()?.Replace(" ", "_");
                serverInfo.Name = instanceName ?? "";
                string? instanceFolder = InstanceFolderDefault + @"\" + instanceName;
                if (Directory.Exists(instanceFolder) || instanceName == null) { Console.WriteLine("Already exists or null"); return -1; }
                Directory.CreateDirectory(instanceFolder);

                Console.WriteLine("Create " + instanceName + " on " + instanceFolder);

                List<string> versions = await GetAllVersionsAsync(MinecraftVersionType.Both);

                Console.WriteLine("Input Version");
                string? versionIn = Console.ReadLine();
                if (!versions.Contains(versionIn)) { Console.WriteLine("Please input available version"); return -1; }

                Console.WriteLine("Set Version " + versionIn);
                serverInfo.Version = versionIn;

                ServerType serverType = (ServerType)Enum.Parse(typeof(ServerType), Console.ReadLine());

                using (WebClient client = new())
                {

                    client.DownloadProgressChanged += (sender, e) =>
                    {
                        Console.Write($"\r{e.BytesReceived} / {e.TotalBytesToReceive} ({e.ProgressPercentage}%)");
                    };

                    switch (serverType)
                    {
                        case ServerType.Vanilla:
                            Console.WriteLine("Now downloading necessary things...");
                            await DownloadAsync(client, await GetServerDownloadURL(versionIn), instanceFolder, "server.jar");
                            break;
                        case ServerType.Forge:
                            await BuildForge(versionIn, client, instanceFolder);
                            break;
                        case ServerType.Fabric:
                            break;
                        case ServerType.Spigot:
                            break;
                        case ServerType.Paper:
                            break;
                        case ServerType.Sponge:
                            break;
                        case ServerType.Magma:
                            break;
                    }

                }

                break;
            default:
                break;
        }
        return 0;
    }

    public static async Task BuildForge(string versionIn, WebClient client, string instanceFolder)
    {
        JObject mavenManifest = await GetResponseAsJObjectAsync(FORGE_MAVEN_VERSION_MANIFEST_URL);
        JToken availableVersions = mavenManifest[versionIn];

        availableVersions.ToList().ForEach(version => Console.WriteLine(version));

        string forgeVersion = Console.ReadLine();
        if (availableVersions.Contains(forgeVersion)) return;

        string forgeUrl = GetForgeDownloadURLFromVersion(forgeVersion);
        string forgeJarName = GetFilenameFromWebServer(forgeUrl);
        await DownloadAsync(client, forgeUrl, instanceFolder, forgeJarName);

        Console.WriteLine();

        using (Process p = new())
        {
            ProcessStartInfo info = new()
            {
                WorkingDirectory = instanceFolder,
                UseShellExecute = false,
                FileName = "java.exe",
                Arguments = $@"-jar {forgeJarName} --installServer {instanceFolder}"
            };
            p.StartInfo = info;
            p.OutputDataReceived += (sender, e) => Console.WriteLine(e.Data);

            p.Exited += (sender, e) => Console.WriteLine("INSTALL SUCCESS");

            Console.WriteLine($@"{info.FileName} {info.Arguments}");

            p.Start();
        }

        Console.WriteLine("Run server? [y/n]");
        switch (Console.ReadLine())
        {
            case "y": break;
            case "n": return;
            default: return;
        }

        Console.WriteLine("Are you agree Minecraft EULA [y/n]? Please Read https://account.mojang.com/documents/minecraft_eula");
        switch (Console.ReadLine())
        {
            case "y":
                string eulaLoc = $@"{instanceFolder}\eula.txt";
                if (File.Exists(eulaLoc)) File.Delete(eulaLoc);

                using (FileStream stream = File.Create(eulaLoc))
                {
                    byte[] eulaAgreement = new UTF8Encoding(true).GetBytes("eula=true");
                    stream.Write(eulaAgreement, 0, eulaAgreement.Length);
                }
                break;
            case "n": return;
            default: return;
        }

        MinecraftVersion? versionMC = ConvertMinecraftVersionFromString(versionIn);
        if (versionMC == null) return;
        bool isBefore1_13 = false;
        bool isJarOldFormat = false;
        if (versionMC.MinorVersion < 12 || (versionMC.MinorVersion == 12 && versionMC.RevisionVersion < 2)) isJarOldFormat = true;
        if (versionMC.MinorVersion < 13) isBefore1_13 = true;

        Console.WriteLine($"Is Before 1.13 = {isBefore1_13}");
        Console.ReadKey();

        using (Process p = new())
        {
            if (isBefore1_13)
            {

                string jarName = "";
                if (isJarOldFormat) jarName = $@"forge-{forgeVersion}-universal.jar";
                else if (isBefore1_13) jarName = $@"forge-{forgeVersion}.jar";

                ProcessStartInfo info = new()
                {
                    UseShellExecute = false,
                    WorkingDirectory = instanceFolder,
                    FileName = "java",
                    Arguments = $@"-jar {jarName} nogui"
                };
                p.StartInfo = info;
                p.OutputDataReceived += (sender, e) => Console.WriteLine(e.Data);
                p.Start();
            }
        }
    }

    public static async Task DownloadAsync(WebClient client, string url, string directory, string fileName)
    {
        if (directory.EndsWith("/")) directory.Substring(0, directory.Length - 1);
        string downloadPath = $@"{directory}/{fileName}";
        await client.DownloadFileTaskAsync(new Uri(url), downloadPath);
    }

    public static string GetFilenameFromWebServer(string url)
    {
        string result = "";

        var req = System.Net.WebRequest.Create(url);
        req.Method = "HEAD";
        using (System.Net.WebResponse resp = req.GetResponse())
        {
            if (!string.IsNullOrEmpty(resp.Headers["Content-Disposition"]))
            {
                result = resp.Headers["Content-Disposition"].Substring(resp.Headers["Content-Disposition"].IndexOf("filename=") + 9).Replace("\"", "");
            }
        }

        return result;
    }

    public async static Task MainAsync(string[] args)
    {

        string url = await GetServerDownloadURL("1.17.1");

        Console.WriteLine(url);

        await Run(args);
        //await Test();
    }

    public static void Main(string[] args)
    {
        MainAsync(args).GetAwaiter().GetResult();
    }
}
