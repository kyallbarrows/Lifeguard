using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Lifeguard
{
    class Token {
        public string token { get; set; }
    }

    class ApiInteractions
    {
        public static string GetToken(string username, string password)
        {
            var client = new HttpClient();

            var byteArray = Encoding.ASCII.GetBytes(username + ":" + password);
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

            // Get the response.
            var uri = ConfigRepo.GetTokenUri();
            try
            {
                var formContent = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("username", username),
                    new KeyValuePair<string, string>("password", password)
                });

                var myHttpClient = new HttpClient();
                var response = myHttpClient.PostAsync(uri, formContent).Result;

                if (response.StatusCode != System.Net.HttpStatusCode.Moved &&
                    response.StatusCode != System.Net.HttpStatusCode.MovedPermanently &&
                    response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    Logger.LogError("Bad response from token api: " + response.StatusCode);
                    return "";
                }

                // Get the response content.
                HttpContent responseContent = response.Content;

                if (response == null || response.Content == null)
                    return "";

                // Get the stream of the content.
                using (var reader = new StreamReader(responseContent.ReadAsStreamAsync().Result))
                {
                    string output = "";
                    try
                    {
                        output = reader.ReadToEndAsync().Result;
                        var token = JsonConvert.DeserializeObject<Token>(output);
                        return token.token;
                    }
                    catch (Exception e) {
                        Logger.LogError("Bad data from token api: " + output);
                        return "";
                    }
                }
            }
            catch (Exception e)
            {
                Logger.LogException(e);
                //and pass it up
                throw;
            }
        }
    }
}
