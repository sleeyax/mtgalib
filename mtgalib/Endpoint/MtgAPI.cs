using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using mtgalib.Player;
using Newtonsoft.Json;

namespace mtgalib
{
    internal class MtgAPI : BaseEndpoint
    {
        private string _accountSystemId;
        private string _accountSystemSecret;

        public MtgAPI(PlayerEnvironment playerEnvironment)
        {
            _url = "https://api.platform.wizards.com";
            _accountSystemId = playerEnvironment.ClientId;
            _accountSystemSecret = playerEnvironment.ClientId;
        }

        /// <summary>
        /// Send a asynchronous POST request to an endpoint
        /// </summary>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        private async Task<string> PostAsyncTask(string url, NameValueCollection data)
        {
            byte[] responseBytes = await _wc.UploadValuesTaskAsync(url, "POST", data);
            return Encoding.UTF8.GetString(responseBytes);
        }

        /// <summary>
        /// Login asynchronously
        /// </summary>
        /// <param name="data">data to send to the login endpoint</param>
        /// <returns></returns>
        private async Task<dynamic> LoginAsyncTask(NameValueCollection data)
        {
            try
            {
                _wc.Headers.Clear();
                _wc.Headers["Content-Type"] = "application/x-www-form-urlencoded";
                string basic = Helpers.Base64Encode($"{_accountSystemId}:{_accountSystemSecret}");
                _wc.Headers["Authorization"] = $"Basic {basic}";

                string response = await PostAsyncTask($"{_url}/auth/oauth/token", data);

                return JsonConvert.DeserializeObject(response);
            }
            catch (WebException ex)
            {
                // status code is 401 Unauthorized
                if (ex.Response is HttpWebResponse response && response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    using (StreamReader r = new StreamReader(response.GetResponseStream()))
                    {
                        string responseBody = r.ReadToEnd();
                        return JsonConvert.DeserializeObject(responseBody);
                    }
                }

                // unknown error, this should not happen
                throw;
            }
        }

        /// <summary>
        /// Login asynchronously using username & password
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public Task<dynamic> LoginAsyncTask(string username, string password)
        {
            return LoginAsyncTask(new NameValueCollection
            {
                {"grant_type", "password"},
                {"username", username},
                {"password", password}
            });
        }

        /// <summary>
        /// Login asynchronously using refresh token
        /// </summary>
        /// <param name="refreshToken"></param>
        /// <returns></returns>
        public Task<dynamic> LoginAsyncTask(string refreshToken)
        {
            return LoginAsyncTask(new NameValueCollection
            {
                {"grant_type", "refresh_token"},
                {"refresh_token", refreshToken}
            });
        }
    }
}