using Newtonsoft.Json;
using System;
using System.Configuration;
using System.IO;

namespace Lifeguard
{
    class ConfigRepo
    {
        public const string LOGOUT_STRING = "LOGOUT";
        public const string SHUTDOWN_STRING = "SHUTDOWN";
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
            return "https://big-red-barn-django.herokuapp.com";
        }


        public static string GetTokenUri()
        {
            return GetServerUri() + "/api-token-auth/";
        }

        public static string GetScreenshotUri()
        {
            return GetServerUri() + "/api/feedposts/";
        }

        private static String GetConfigPath()
        {
            return Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "phlg",
                    "i" + typeof(LifeguardBackgroundApp).Assembly.GetName().Version.ToString() + ".cfg");
        }

        public static LifeguardConfiguration GetConfig()
        {
            var newConfig = new LifeguardConfiguration();
            newConfig.MachineID = Guid.NewGuid().ToString("N");

            lock (GetLockObject())
            {
                var path = GetConfigPath();
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
                        return newConfig;
                    }
                    catch (Exception e)
                    {
                        Logger.LogException(e);
                    }
                }
            }

            return newConfig;
        }


        public static void SaveConfig(string username, string token)
        {
            var currentConfig = GetConfig();
            var newConfig = new LifeguardConfiguration { Username = username, Token = token, MachineID = currentConfig.MachineID };
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
