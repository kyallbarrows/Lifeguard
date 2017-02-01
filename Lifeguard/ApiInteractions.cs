using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Lifeguard
{
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
                var response = client.GetAsync(uri).Result;

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
                    // Write the output.
                    //strip the " that seem to come along with the token
                    var output = reader.ReadToEndAsync().Result;
                    output = output.Replace("\"", "");
                    return output;
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
