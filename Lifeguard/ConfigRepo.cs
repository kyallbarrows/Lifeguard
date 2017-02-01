using Newtonsoft.Json;
using System;
using System.Configuration;
using System.IO;

namespace Lifeguard
{
    class ConfigRepo
    {
        public const string LOGOUT_STRING = "LOGOUT";
        private static object LockObject;
        private static object GetLockObject()
        {
            if (LockObject == null)
                LockObject = new object();

            return LockObject;
        }

        //TODO: does storing in app.config leave us vulnerable to other apps changing our server setting?
        private const string SERVER_CONFIG_SETTING = "serveruri";
        public static string GetServerUri()
        {
            var appSettings = ConfigurationManager.AppSettings;
            return appSettings[SERVER_CONFIG_SETTING] ?? "";
        }


        public static string GetTokenUri()
        {
            return GetServerUri() + "/wp-json/lifeguard/v1/token";
        }

        public static string GetScreenshotUri()
        {
            return GetServerUri() + "/wp-json/lifeguard/v1/screenshot";
        }

        private static String GetConfigPath()
        {
            return Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "phlg",
                    "i.cfg");
        }

        public static LifeguardConfiguration GetConfig()
        {
            lock (GetLockObject())
            {
                var path = GetConfigPath();
                Console.WriteLine("Encryption Key File: " + path);

                var fileInfo = new FileInfo(path);
                if (fileInfo.Exists)
                {
                    try
                    {
                        string contents = File.ReadAllText(fileInfo.FullName);
                        return JsonConvert.DeserializeObject<LifeguardConfiguration>(contents) ?? new LifeguardConfiguration();
                    }
                    catch (JsonException je)
                    {
                        //config got corrupted somehow, create a new blank one
                        return new LifeguardConfiguration();
                    }
                    catch (Exception e)
                    {
                        Logger.LogException(e);
                    }
                }
            }

            return new LifeguardConfiguration();
        }


        public static void SaveConfig(string username, string token, string machineID)
        {
            var newConfig = new LifeguardConfiguration { Username = username, Token = token, MachineID = machineID };
            SaveConfig(newConfig);
        }

        public static void SaveConfig(LifeguardConfiguration config)
        {
            FileInfo fileInfo;

            lock (GetLockObject())
            {
                var path = GetConfigPath();

                fileInfo = new FileInfo(path);
                if (!fileInfo.Directory.Exists)
                {
                    fileInfo.Directory.Create();
                }

                try
                {
                    string jsonConfig = JsonConvert.SerializeObject(config);
                    File.WriteAllText(fileInfo.FullName, jsonConfig);
                }
                catch (Exception e)
                {
                    Logger.LogException(e);
                }
            }
        }

    }
}
