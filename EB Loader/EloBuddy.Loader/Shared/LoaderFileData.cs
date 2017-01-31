using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using EloBuddy.Loader.Data;
using EloBuddy.Loader.Update;
using EloBuddy.Loader.Utils;
using Newtonsoft.Json;

namespace EloBuddy.Sandbox.Shared
{
    [DataContract]
    public class LoaderFileData
    {
        [DataMember]
        public FileData[] SystemFiles { get; set; }

        [DataMember]
        public FileData[] AddonFiles { get; set; }

        [DataMember]
        public string LoaderPath { get; set; }

        public static LoaderFileData GatherFileData()
        {
            var data = new LoaderFileData()
            {
                LoaderPath = EnvironmentHelper.FileName,
                SystemFiles =
                    JsonConvert.DeserializeObject<UpdateData>(LoaderUpdate.LatestUpdateJson)
                        .StaticFiles.Where(f => Path.GetDirectoryName(f.Key) == "System")
                        .Select(f => new FileData(f.Key, f.Value.MD5))
                        .ToArray(),
                AddonFiles = Settings.Instance.InstalledAddons.Select(a => new FileData(a.GetOutputFilePath(), a.Hash, a.IsBuddyAddon)).ToArray()
            };

            return data;
        }
    }

    [DataContract]
    public class FileData
    {
        [DataMember]
        public string Path { get; set; }

        [DataMember]
        public string Hash { get; set; }

        [DataMember]
        public bool RequiresBuddy { get; set; }

        public FileData(string path, string hash, bool requiresBuddy = false)
        {
            Path = path;
            Hash = hash;
            RequiresBuddy = false;
        }
    }
}
