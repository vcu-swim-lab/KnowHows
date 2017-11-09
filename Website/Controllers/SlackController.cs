using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using System.Web.Http;

namespace Website.Controllers
{
    public class SlackController : ApiController
    {
        private const string SLACK_APP_CLIENT_ID = "";
        private const string SLACK_APP_CLIENT_SECRET = "";

        private const string SLACK_APP_OAUTH_URL = "https://slack.com/oauth/authorize";
        private const string SLACK_APP_OAUTH_REDIRECT_URL = "";
        private const string SLACK_APP_OAUTH_SCOPE = "bot commands client";

        public class SlashCommand
        {
            public string token             { get; set; }
            public string team_id           { get; set; }
            public string team_domain       { get; set; }
            public string enterprise_id     { get; set; }
            public string enterprise_name   { get; set; }
            public string channel_id        { get; set; }
            public string channel_name      { get; set; }
            public string user_id           { get; set; }
            public string user_name         { get; set; }
            public string command           { get; set; }
            public string text              { get; set; }
            public string response_url      { get; set; }
            public string trigger_id        { get; set; }
        }

        private class OAuthResponse
        {
            public string access_token, scope;
        }

        public IHttpActionResult ProcessMessage([FromBody] SlashCommand command)
        {
            // @TODO: verify message is actually from slack via verification token

            // @TODO: process slash command

            // @TODO: proper error code based on command execution
            return Ok();
        }

        public IHttpActionResult Authenticate(string code, string state)
        {
            Console.WriteLine("Receieved install: {0} {1}", code, state);

            // @TODO: save the token and associate it with something
            OAuthResponse response = RequestAccessToken(code);
            Console.WriteLine("Received Slack OAuth: {0}, {1}", code, response.access_token);

            // @TODO: change this once we have an idea where we want to redirect users after installing the app
            // perhaps a tutorial page showing how to use the commands/app?
            return Redirect("google.com");
        }

        private OAuthResponse RequestAccessToken(string code)
        {
            WebClient client = new WebClient();
            client.Headers["Accept"] = "application/json";

            string uri = String.Format("https://slack.com/api/oauth.access?client_id={0}&client_secret={1}&code={2}", SLACK_APP_CLIENT_ID, SLACK_APP_CLIENT_SECRET, code);
            string response = client.DownloadString(new Uri(uri));

            try { return JsonConvert.DeserializeObject<OAuthResponse>(response); }
            catch (Exception ex)
            {
                throw new Exception(String.Format("Failed to process OAuth response to request for code {0}", code));
            }
        }

        public string GetOAuthURL()
        {
            return SLACK_APP_OAUTH_URL +
                String.Format("?client_id={0}&redirect_uri={1}&scope={2}&state={3}",
                SLACK_APP_CLIENT_ID,
                SLACK_APP_OAUTH_REDIRECT_URL,
                SLACK_APP_OAUTH_SCOPE,
                "state");
        }
    }
}
