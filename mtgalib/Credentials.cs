using System;
using System.Net;
using System.Security;
using System.Threading.Tasks;

namespace mtgalib
{
    public class Credentials
    {

        private MtgAPI _api = new MtgAPI();
        private readonly string _password;
        private readonly string _username;
        public string RefreshToken { get; private set; }
        public string AccessToken { get; private set; }
        public string DisplayName { get; private set; }
        public string PersonaId { get; private set; }


        public Credentials(string username, string password)
        {
            _username = username;
            _password = password;
        }

        public Credentials(string refreshToken)
        {
            RefreshToken = refreshToken;
        }

        public async Task<bool> VerifyAsync()
        {
            dynamic jsonResponse = RefreshToken != null ? await _api.LoginAsync(RefreshToken) : await _api.LoginAsync(_username, _password);

            if (jsonResponse.code == 401)
                return false;

            AccessToken = jsonResponse.access_token;
            RefreshToken = jsonResponse.refresh_token;
            DisplayName = jsonResponse.display_name;
            PersonaId = jsonResponse.persona_id;

            return true;
        }
    }
}