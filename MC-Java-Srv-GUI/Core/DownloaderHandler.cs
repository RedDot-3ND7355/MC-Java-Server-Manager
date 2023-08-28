using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Net;

namespace MC_Java_Srv_GUI.Core
{
    public static class DownloaderHandler
    {
        // Version -> Json URL
        public static Dictionary<string, string> VersionList = new Dictionary<string, string>();
        public static string LatestVersion = "";

        public static void Ini() =>
            DownloadVersionList();

        private static void DownloadVersionList()
        {
            string contents = "";
            using (var wc = new WebClient())
                contents = wc.DownloadString("https://launchermeta.mojang.com/mc/game/version_manifest.json");
            JObject obj = JsonConvert.DeserializeObject<JObject>(contents);
            /* ==== GET VERSION TABLE ==== */
            LatestVersion = obj["latest"]["release"].ToString(); // Get latest version
            foreach (JObject pos in obj["versions"])
                // Check if version is of type release
                if (pos["type"].ToString() == "release")
                {
                    // Get Download URL via json url
                    string content = "";
                    using (var wc = new WebClient())
                        content = wc.DownloadString(pos["url"].ToString());
                    JObject obj2 = JsonConvert.DeserializeObject<JObject>(content);
                    // Add version with download url
                    if (obj2["downloads"]["server"] != null)
                        VersionList.Add(pos["id"].ToString(), obj2["downloads"]["server"]["url"].ToString());
                }
        }
    }
}
