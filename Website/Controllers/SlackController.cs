using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using static Website.Utility.OAuth;

namespace Website.Controllers
{
    [Route("api/slack")]
    public class SlackController : Controller
    {
        private const string SLACK_APP_CLIENT_ID = "";
        private const string SLACK_APP_CLIENT_SECRET = "";

        private const string SLACK_APP_OAUTH_URL = "https://slack.com/oauth/authorize";
        private const string SLACK_APP_OAUTH_ACCESS_URL = "https://slack.com/api/oauth.access";

        private static string SLACK_APP_OAUTH_REDIRECT_URL = Program.WEBSITE_BASE_URL + "/api/slack/authenticate";

        private const string SLACK_APP_OAUTH_SCOPE = "client";

        public class SlashCommand
        {
            public string token { get; set; }
            public string team_id { get; set; }
            public string team_domain { get; set; }
            public string enterprise_id { get; set; }
            public string enterprise_name { get; set; }
            public string channel_id { get; set; }
            public string channel_name { get; set; }
            public string user_id { get; set; }
            public string user_name { get; set; }
            public string command { get; set; }
            public string text { get; set; }
            public string response_url { get; set; }
            public string trigger_id { get; set; }
        }

        [HttpPost]
        [Route("ProcessMessage")]
        public IActionResult ProcessMessage(SlashCommand command)
        {
            // @TODO: verify message is actually from slack via verification token

            // @TODO: process slash command

            // @TODO: proper error code based on command execution
            return Ok();
        }

        [HttpGet]
        [Route("Authenticate")]
        public IActionResult Authenticate(OAuthRequest request)
        {
            Console.WriteLine("Received Slack install: '{0}' '{1}'", request.code, request.state);

            // @TODO: save the token and associate it with something
            OAuthResponse response = RequestAccessToken(request.code);
            Console.WriteLine("Received Slack OAuth: '{0}', '{1}'", request.code, response.access_token);

            // @TODO: change this once we have an idea where we want to redirect users after installing the app
            // perhaps a tutorial page showing how to use the commands/app?
            return Redirect(Url.Content("~/"));
        }

        private OAuthResponse RequestAccessToken(string code)
        {
            WebClient client = new WebClient();
            client.Headers["Accept"] = "application/json";

            string uri = SLACK_APP_OAUTH_ACCESS_URL + String.Format("?client_id={0}&client_secret={1}&caaode={2}&redirect_uri={3}", SLACK_APP_CLIENT_ID, SLACK_APP_CLIENT_SECRET, code, SLACK_APP_OAUTH_REDIRECT_URL);
            string response = client.DownloadString(new Uri(uri));

            try { return JsonConvert.DeserializeObject<OAuthResponse>(response); }
            catch (Exception ex)
            {
                throw new Exception(String.Format("Failed to process Slack OAuth Access Token response for code: '{0}', reason: {1}", code, ex.ToString()));
            }
        }

        [Route("getOAuthURL")]
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
