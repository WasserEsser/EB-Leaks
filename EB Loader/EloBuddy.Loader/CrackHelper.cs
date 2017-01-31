using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using Newtonsoft.Json;
using System.IO;
using System.Diagnostics;
using EloBuddy.Loader.Utils;

namespace EloBuddy.Loader
{
    public static class CrackHelper
    {
        public static string GetLatestLoaderHash()
        {
            string json = "";
            using (WebClient wc = new WebClient())
            {
                json = wc.DownloadString("https://raw.githubusercontent.com/EloBuddy/EloBuddy.Dependencies/master/dependencies.json");
            }

            Update.UpdateData d = JsonConvert.DeserializeObject<Update.UpdateData>(json);

            return d.Loader.MD5;
        }

        public static string GetLatestLoaderVersion()
        {
            if (GetLatestLoaderHash() == Md5Hash.Compute(File.ReadAllBytes(EnvironmentHelper.FileName))) {
                return EnvironmentHelper.GetAssemblyVersion().ToString();
            }
            string json = "";
            using (WebClient wc = new WebClient())
            {
                json = wc.DownloadString("https://raw.githubusercontent.com/EloBuddy/EloBuddy.Dependencies/master/dependencies.json");
            }

            Update.UpdateData d = JsonConvert.DeserializeObject<Update.UpdateData>(json);

            string temppath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "latest.exe");

            using (WebClient wc = new WebClient())
            {
                wc.DownloadFile(d.Loader.Download, temppath);
            }

            string vers = FileVersionInfo.GetVersionInfo(temppath).ProductVersion;

            File.Delete(temppath);

            return vers;
        }
    }
}
