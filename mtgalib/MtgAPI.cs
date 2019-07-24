using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Security.Policy;
using System.Text;
using Newtonsoft.Json;

namespace mtgalib
{
    public class MtgAPI
    {
        private readonly string _url = "https://api.platform.wizards.com";
        // clientId
        private readonly string _accountSystemId = "N8QFG8NEBJ5T35FB";
        // clientSecret
        private readonly string _accountSystemSecret = "VMK1RE8YK6YR4EABJU91";
        private WebClient _wc;


        public MtgAPI()
        {
            _wc = new WebClient();
        }

        /// <summary>
        /// Send a POST request to an endpoint
        /// </summary>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        private string Post(string url, NameValueCollection data)
        {
            byte[] responseBytes = _wc.UploadValues(url, "POST", data);
            return Encoding.UTF8.GetString(responseBytes);
        }

        /// <summary>
        /// Login
        /// </summary>
        /// <param name="data">data to send to the login endpoint</param>
        /// <returns></returns>
        private object Login(NameValueCollection data)
        {
            try
            {
                _wc.Headers.Clear();
                _wc.Headers["Content-Type"] = "application/x-www-form-urlencoded";
                string basic = Helpers.Base64Encode($"{_accountSystemId}:{_accountSystemSecret}");
                _wc.Headers["Authorization"] = $"Basic {basic}";

                string response = Post($"{_url}/auth/oauth/token", data);

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
        /// Login using username & password
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public object Login(string username, string password)
        {
            NameValueCollection data = new NameValueCollection
            {
                {"grant_type", "password"},
                {"username", username},
                {"password", password},
            };

            return Login(data);
        }

        /// <summary>
        /// Login using refresh token
        /// </summary>
        /// <param name="refreshToken"></param>
        /// <returns></returns>
        public dynamic Login(string refreshToken)
        {
            NameValueCollection data = new NameValueCollection
            {
                {"grant_type", "refresh_token"},
                {"refresh_token", refreshToken}
            };

            return Login(data);
        }
    }
}