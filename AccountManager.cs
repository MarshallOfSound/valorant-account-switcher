using System;
using System.Collections.Generic;
using System.IO;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace ValorantAccountSwitcher
{
    class AccountManager
    {
        private static string RiotDataDirectory = Path.GetFullPath(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Riot Games", "Riot Client", "Data"));
        private static string RiotSettingsFile = "RiotClientPrivateSettings.yaml";
        private static string AccountDirectory = Path.GetFullPath(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Valorant Account Switcher"));

        class RiotSettings
        {
            public PrivateSettings _private { get; set; }
        }
        class PrivateSettings
        {
            public RiotLogin riotLogin { get; set; }
        }

        class RiotLogin
        {
            public Persist persist { get; set; }
        }

        class Persist
        {
            public string region { get; set; }
            public RiotSession session { get; set; }
        }

        class RiotSession
        {
            public List<RiotCookie> cookies { get; set; }
        }

        class RiotCookie
        {
            public string name { get; set; }
            public string value { get; set; }
        }

        private static string GetUniqueIdentifierForCreds(string filename)
        {
            var contents = File.ReadAllText(filename);
            var deserializer = new DeserializerBuilder().WithAttributeOverride<PrivateSettings>(
                c => c.riotLogin,
                new YamlMemberAttribute
                {
                    Alias = "riot-login"
                }
            ).IgnoreUnmatchedProperties().Build();
            var settings = deserializer.Deserialize<RiotSettings>(new StringReader("_" + contents));
            foreach (var cookie in settings._private.riotLogin.persist.session.cookies)
            {
                Console.WriteLine("Cookie: " + cookie.name);
                if (cookie.name == "sub") return cookie.value;
            }

            return "Complete_Unknown";
        }

        public static List<string> GetAccountNames()
        {
            EnsureAccounts();
            List<string> names = new List<string>();
            foreach (string file in Directory.GetFiles(AccountDirectory))
            {
                if (Path.GetExtension(file) == ".yaml")
                {
                    names.Add(Path.GetFileNameWithoutExtension(file));
                }
            }
            return names;
        }

        public static string GetAccountFilePath(string account)
        {
            EnsureAccounts();
            return Path.Combine(AccountDirectory, account + ".yaml");
        }

        public static bool IsCurrentAccountUnknown()
        {
            if (!Directory.Exists(RiotDataDirectory)) return false;
            string settingsPath = Path.Combine(RiotDataDirectory, RiotSettingsFile);
            if (!File.Exists(settingsPath)) return false;

            string currentId = GetUniqueIdentifierForCreds(settingsPath);
            foreach (string account in GetAccountNames())
            {
                if (GetUniqueIdentifierForCreds(GetAccountFilePath(account)) == currentId) return false;
            }

            return true;
        }

        public static string GetCurrentAccountName()
        {
            if (!Directory.Exists(RiotDataDirectory)) return "";
            string settingsPath = Path.Combine(RiotDataDirectory, RiotSettingsFile);
            if (!File.Exists(settingsPath)) return "";

            string currentId = GetUniqueIdentifierForCreds(settingsPath);
            foreach (string account in GetAccountNames())
            {
                if (GetUniqueIdentifierForCreds(GetAccountFilePath(account)) == currentId) return account;
            }

            return "";
        }

        public static void CopyCurrentToAccount(string account)
        {
            EnsureAccounts();
            if (!Directory.Exists(RiotDataDirectory)) return;
            string settingsPath = Path.Combine(RiotDataDirectory, RiotSettingsFile);
            if (!File.Exists(settingsPath)) return;
            string accountPath = GetAccountFilePath(account);

            if (File.Exists(accountPath))
            {
                File.Delete(accountPath);
            }
            File.Copy(settingsPath, GetAccountFilePath(account));
        }

        public static void CopyAccountToCurrent(string account)
        {
            EnsureAccounts();
            if (!Directory.Exists(RiotDataDirectory)) return;
            string settingsPath = Path.Combine(RiotDataDirectory, RiotSettingsFile);
            if (!File.Exists(settingsPath)) return;
            string accountPath = GetAccountFilePath(account);
            if (!File.Exists(accountPath)) return;

            File.Delete(settingsPath);
            File.Copy(accountPath, settingsPath);
        }

        private static void EnsureAccounts()
        {
            if (!Directory.Exists(AccountDirectory))
            {
                Directory.CreateDirectory(AccountDirectory);
            }
        }
    }
}
