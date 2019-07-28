using System.Threading.Tasks;

namespace mtgalib.Player
{
    public class PlayerCredentials
    {

        private MtgAPI _api = new MtgAPI();
        private readonly string _password;
        private readonly string _username;
        public string RefreshToken { get; private set; }
        public string AccessToken { get; private set; }
        public string DisplayName { get; private set; }
        public string PersonaId { get; private set; }


        public PlayerCredentials(string username, string password)
        {
            _username = username;
            _password = password;
        }

        public PlayerCredentials(string refreshToken)
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