using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

using Website.Utility;
using Website.Utility.OAuth;
using Website.Manager;

namespace Website.Controllers
{
    [Route("api/slack")]
    public class SlackController : Controller
    {
        private const string SLACK_APP_OAUTH_URL = "https://slack.com/oauth/authorize";
        private const string SLACK_APP_OAUTH_ACCESS_URL = "https://slack.com/api/oauth.access";
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

        private readonly AppSettings _options;
        public SlackController(IOptions<AppSettings> optionsAccessor)
        {
            _options = optionsAccessor.Value;
        }

        private OAuthResponse RequestAccessToken(string code)
        {
            WebClient client = new WebClient();
            client.Headers["Accept"] = "application/json";

            string uri = SLACK_APP_OAUTH_ACCESS_URL + String.Format("?client_id={0}&client_secret={1}&caaode={2}&redirect_uri={3}",
                _options.SLACK_APP_CLIENT_ID,
                _options.SLACK_APP_CLIENT_SECRET,
                code,
                _options.SLACK_APP_OAUTH_REDIRECT_URL);

            string response = client.DownloadString(new Uri(uri));

            try { return JsonConvert.DeserializeObject<OAuthResponse>(response); }
            catch (Exception ex)
            {
                throw new Exception(String.Format("Failed to process Slack OAuth Access Token response for code: '{0}', reason: {1}", code, ex.ToString()));
            }
        }

        private Dictionary<String, String> GetParametersFromRequest()
        {
            String message;

            // Read request body into string
            using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8))
                message = reader.ReadToEnd().ToString();

            // Parse message into dictionary
            var dictionary = message
                            .Split("&")
                            .Select(s => s.Split('='))
                            .ToDictionary
                            (
                                key => WebUtility.UrlDecode(key[0].Trim()),
                                value => WebUtility.UrlDecode(value[1].Trim())
                            );

            return dictionary;
        }

        [HttpPost]
        [Route("ProcessMessage")]
        public String ProcessMessage()
        {
            // @TODO: Figure out why the slash command can't be properly deserialized when passed as an argument
            // as is, we have to do this hacky way to actualize our slash command from Slack
            var parameters = GetParametersFromRequest();

            foreach (var k in parameters.Keys)
                Console.WriteLine("{0}: {1}", k, parameters[k]);

            // @TODO: verify message is actually from slack via verification token

            // @TODO: process slash command

            // @TODO: proper error code based on command execution

            string uuid = parameters["team_id"] + "." + parameters["channel_id"] + "." + parameters["user_id"];

            if(UserManager.Instance.IsGitHubAuthenticated(uuid))
            {
                GitHubUser user = UserManager.Instance.GetGitHubUser(uuid);
                StringBuilder sb = new StringBuilder();

                sb.AppendLine("Successfully authenticated with GitHub.");
                sb.AppendLine("Here are the repositories we found from your profile:");
                foreach (String repo in user.Repositories) sb.AppendLine(repo);

                sb.AppendLine();
                sb.AppendLine(String.Format("Additionally, we recieved your command with text: {0}", parameters["text"]));

                return sb.ToString();
            }
            else
                return String.Format("It looks like you haven't authorized us as a GitHub app in this channel! Please visit this URL to get set up: {0}/api/github/getoauthurl?uuid={1}", 
                        _options.WEBSITE_BASE_URL, 
                        uuid);
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

        [HttpGet]
        [Route("getOAuthURL")]
        public IActionResult GetOAuthURL()
        {
            return Redirect(SLACK_APP_OAUTH_URL +
                    String.Format("?client_id={0}&redirect_uri={1}&scope={2}&state={3}",
                    _options.SLACK_APP_CLIENT_ID,
                    _options.SLACK_APP_OAUTH_REDIRECT_URL,
                    SLACK_APP_OAUTH_SCOPE,
                    "state"));
        }
    }
}
