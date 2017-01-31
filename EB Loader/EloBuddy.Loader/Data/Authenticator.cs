using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;
using EloBuddy.Loader.Logger;
using EloBuddy.Loader.Networking;
using EloBuddy.Loader.Update;
using EloBuddy.Loader.Utils;
using EloBuddy.Networking;
using EloBuddy.Networking.Objects;
using System.Windows.Forms;

namespace EloBuddy.Loader.Data
{
    internal static class Authenticator
    {
        private const string PasswordPublicKey =
            "BgIAAACkAABSU0ExAAoAAAEAAQAJo0rgJOIz1OV0hvirueBnU82OhGoyyM9s6QRVDjl/TZjjO69aodyJ970GK3fPp+9/MYbZKccSuye13m4IdWgq4ppeFTLaTymqjatBsC6oj/vOqk9RzwUo7i/egLN/udWNgBztk62eORHKY9U4OD+0Rg5dcTq+cWt/mZo1LEiACUZU0pXV5IrY21qGnM7P5FKtuGIWrXRHCoLL2IAP1kDa120wmoqnerazUIsLwSc0N2ozhdCOoo9fLECyc2O4Yb/8co8G2gfPLOQy6ZoRmAMmNyt2K1RU1XPrVlhMzdfk2nv2krpv8pyS1AFazOEUnJtZ9sjoURe2UvusJb1IrDxvdTRKfKWj2WN28oj+XLp6oj+Rol6hycdubnysDXuQwpPOk41U17sU2P9gD/erukaqeAeD+yE4VXFOmOf/NW+YsQ==";

        internal static string LastError { get; private set; }

        private static string _hwid;
        internal static string Hwid
        {
            get
            {
                if (string.IsNullOrEmpty(_hwid))
                {
                    _hwid = EnvironmentHelper.GetMachineGuid();
                }

                return _hwid;
            }
        }

        internal static int[] PremiumGroups
        {
            get
            {
                return new[]
                {
                    7, //Addon Developer
                    4, //Administrators
                    8, //Buddy
                    10, //Community Moderator
                    13, //Community Support
                    9, //Contributor
                    11, //Developer
                    21, //GFX Designer
                    12, //Local Support
                    20, //Trial Local Support
                    6, //Moderator
                    18, //Retired staff
                };
            }
        }

        internal static bool IsBuddy
        {
            get { return PremiumGroups.Contains(GroupId); }
        }

        internal static Credentials Credentials { get; private set; }

        internal static Image Avatar { get; private set; }

        internal static string DisplayName { get; private set; }

        internal static string GroupName { get; private set; }

        internal static int GroupId { get; private set; }

        internal static byte[] Token { get; private set; }

        internal static byte[] Signature { get; private set; }

        internal static bool Login(string username, string password, bool autoLogin = false)
        {
            if (!autoLogin)
            {
                using (var rsaProvider = new RSACryptoServiceProvider(new CspParameters { ProviderType = 1 }))
                {
                    rsaProvider.ImportCspBlob(Convert.FromBase64String(PasswordPublicKey));
                    password = Convert.ToBase64String(rsaProvider.Encrypt(Encoding.Default.GetBytes(password), false));
                }
            }

            try
            {
                var client = new EbClient();
                var netAuthPacket = client.Do((byte)Headers.Authentication,
                    new object[]
                    {
                        new AuthRequest
                        {
                            Username = username,
                            Password = password,
                            Version = CrackHelper.GetLatestLoaderVersion(), //EnvironmentHelper.GetAssemblyVersion().ToString(),
                            Hash = CrackHelper.GetLatestLoaderHash(), //Md5Hash.Compute(File.ReadAllBytes(EnvironmentHelper.FileName)),
                            Hwid = "MYFAKEHWID",
                            LeagueVersion = LoaderUpdate.LeagueVersion,
                            Params = new object[] { (int?) Settings.Instance.SelectedLanguage, Settings.Instance.Directories.AppDataDirectory, Settings.Instance.DeveloperMode }
                        }
                    });
                client.Close();
                client.InnerChannel.Dispose();

                if (netAuthPacket.Success)
                {
                    AuthResponse response;
                    using (var stream = new MemoryStream(netAuthPacket.Data))
                    {
                        var dataContractSerializer = new DataContractSerializer(typeof (AuthResponse));
                        response = (AuthResponse) dataContractSerializer.ReadObject(stream);
                    }

                    DisplayName = "finndev:^)";
                    Avatar = response.Avatar != null ? Image.FromStream(new MemoryStream(response.Avatar)) : null;
                    GroupName = "Administrators";
                    GroupId = 4;
                    Token = response.Token;
                    Signature = response.Data;

                    Credentials = new Credentials(username, password);
                }
                else
                {
                    LastError = netAuthPacket.Body;
                    //NLog.Error("Failed login: \"{0}\" with error: \"{1}\"", username, netAuthPacket.Body);
                }

                return netAuthPacket.Success;
            }
            catch (Exception e)
            {
                Log.Instance.DoLog(string.Format("Exception while trying to authenticate. Exception: {0}", e.Message), Log.LogType.Error);
                Log.Instance.DoLog("Authenticating as guest.");

                // Fail safe
                DisplayName = "Guest";
                Credentials = new Credentials(username, "");
                LastError = "Unable to reach the server!";

                return true;
            }
        }
    }
}
