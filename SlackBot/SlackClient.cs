using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

using SlackAPI;
using System.Net;

namespace SlackBot
{
    class SlackClient
    {
        // Slack requires a redirect URL even though the docs say that it doesn't... 
        const String REDIRECT_URL = "http://localhost/";

        private SlackAPI.SlackClient client;

        public SlackClient(String clientId, String clientSecret, String teamName)
        {
            SetupClient(clientId, clientSecret, teamName);
        }

        // this implemtation is temporary, we need to address the issues dealing with slacks lame oauth 
        private void SetupClient(String clientId, String clientSecret, String teamName)
        {
            var state = Guid.NewGuid().ToString();

            String asString = GetAuthenticationURL(clientId, teamName);
            Console.WriteLine(asString);

            var qs = HttpUtility.ParseQueryString(asString);
            var code = qs["code"];

            if(code == null || code.Length == 0) {
                throw new Exception("Failed to authenticate with Slack");
            }

            Console.WriteLine("Requesting access token...");

            SlackAPI.SlackClient.GetAccessToken((response) =>
            {
                var accessToken = response.access_token;
                Console.WriteLine("Got access token '{0}'...", accessToken);

                this.client = new SlackAPI.SlackClient(accessToken);

            }, clientId, clientSecret, REDIRECT_URL, code);
        }

        private String GetAuthenticationURL(String clientId, String teamName)
        {
            var uri = SlackAPI.SlackClient.GetAuthorizeUri(clientId, SlackScope.Identify | SlackScope.Read | SlackScope.Post, team: teamName);

            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(uri);
            webRequest.AllowAutoRedirect = false;
            webRequest.Timeout = 10000;

            using (var webResponse = (HttpWebResponse) webRequest.GetResponse())
            {
                // Since we should be redirected, return our generated redirect provided by Slack
                // our auth code should be a parameter that can be found in this response URL if
                // we authenticated successfully
                if ((int) webResponse.StatusCode >= 300 && (int) webResponse.StatusCode <= 399) {
                    return webResponse.ResponseUri.ToString();
                }
            }

            return null;
        }
    }
}
