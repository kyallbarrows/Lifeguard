using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lifeguard
{
    class ConfigRepo
    {
        private static LifeguardConfiguration config;

        private static FileInfo GetFile()
        {
            var path = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "phlg",
                "i.cfg");

            Console.WriteLine("Encryption Key File: " + path);

            var file = new FileInfo(path);
            if (!file.Directory.Exists)
                file.Directory.Create();

            return file;
        }

        private static void LoadConfig() {
            var file = GetFile();
            if (file.Exists)
            {
                string contents = File.ReadAllText(file.FullName);
                try
                {
                    //TODO: decrypt the encrypted stuff
                    config = JsonConvert.DeserializeObject<LifeguardConfiguration>(contents);
                }
                catch (JsonException je) {
                    //config got corrupted somehow, create a new blank one
                    config = new LifeguardConfiguration();
                }
            }
        }

        public static string GetUsername() {
            if (config == null)
                LoadConfig();
            return config.Username;
        }

        public static string GetToken()
        {
            if (config == null)
                LoadConfig();
            return config.Token;
        }

        public static string GetMachineID() {
            if (config == null)
                LoadConfig();
            return config.MachineID;
        }

        public static void StoreConfig(string username, string token, string machineID)
        {
            var newConfig = new LifeguardConfiguration { Username = username, Token = token, MachineID = machineID };
            string jsonConfig = JsonConvert.SerializeObject(newConfig);
            var file = GetFile();
            File.WriteAllText(file.FullName, jsonConfig);
        }

        /*        private static byte[] GetEncryptionKey()
                {

                    // determine if current user of machine
                    // or any user of machine can decrypt the key
                    var scope = DataProtectionScope.CurrentUser;

                    // make it a bit tougher to decrypt 
                    var entropy = Encoding.UTF8.GetBytes("correct horse battery staple :)");

                    if (file.Exists)
                    {
                        return ProtectedData.Unprotect(
                            File.ReadAllBytes(path), entropy, scope);
                    }

                    // generate key
                    byte[] key = new byte[1024];
                    using (var rng = RNGCryptoServiceProvider.Create())
                    {
                        rng.GetBytes(key);
                    }
                    // encrypt the key
                    var encrypted = ProtectedData.Protect(key, entropy, scope);

                    // save for later use   
                    File.WriteAllBytes(path, encrypted);

                    return key;
                }*/
    }

}
