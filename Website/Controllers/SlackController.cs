using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Reflection;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

using Website.Utility;
using Website.Utility.OAuth;
using Website.Manager;
using Website.Commands;

namespace Website.Controllers
{
    [Route("api/slack")]
    public class SlackController : Controller
    {
        private const string SLACK_APP_OAUTH_URL = "https://slack.com/oauth/authorize";
        private const string SLACK_APP_OAUTH_ACCESS_URL = "https://slack.com/api/oauth.access";
        private const string SLACK_APP_OAUTH_SCOPE = "client";

        private readonly AppSettings _options;
        public SlackController(IOptions<AppSettings> optionsAccessor)
        {
            _options = optionsAccessor.Value;
        }

        private OAuthResponse RequestAccessToken(string code)
        {
            WebClient client = new WebClient();
            client.Headers["Accept"] = "application/json";

            string uri = String.Format("{0}?client_id={1}&client_secret={2}&caaode={3}&redirect_uri={4}",
                SLACK_APP_OAUTH_ACCESS_URL,
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

        private Command GetCommandFromRequestParameters(Dictionary<string, string> dictionary)
        {
             // Parse keys and values into command using reflection
            Command c = new Command();
            
            foreach (var k in dictionary.Keys) {
                PropertyInfo propertyInfo = typeof(Command).GetProperty(k);
                propertyInfo.SetValue(c, dictionary[k], null);
            }

            return c;
        }

        private Dictionary<string, string> GetParametersFromRequest()
        {
            String message;

            // Read request body into string
            using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8)) {
                message = reader.ReadToEnd().ToString();
            }

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
        public CommandResponse ProcessMessage()
        {
            // @TODO: Figure out why the slash command can't be properly deserialized when passed as an argument
            // as is, we have to do this hacky way to actualize our slash command from Slack
            var parameters = GetParametersFromRequest();
            var command = GetCommandFromRequestParameters(parameters);
            foreach (var k in parameters.Keys) Console.WriteLine("{0}: {1}", k, parameters[k]);

            // @TODO: verify message is actually from slack via verification 
            // @TODO: proper error code based on command execution

            string uuid = parameters["team_id"] + "." + parameters["channel_id"] + "." + parameters["user_id"];

            if(UserManager.Instance.IsGitHubAuthenticated(uuid))
            {
                GitHubUser user = UserManager.Instance.GetGitHubUser(uuid);
                return CommandHandler.HandleCommand(user, command);
            }
            else
            {
                return new CommandResponse(String.Format
                (
                    "{\"text\":"+
                    "It looks like you haven't authorized us as a GitHub app in this channel! " +
                    "Please visit <{0}/api/github/getoauthurl?uuid={1}|this> URL to get set up",
                    _options.WEBSITE_BASE_URL,
                    uuid
                    + "}"
                ));
            }    
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
            return Redirect(String.Format("{0}?client_id={1}&redirect_uri={2}&scope={3}&state={4}",
                SLACK_APP_OAUTH_URL,
                _options.SLACK_APP_CLIENT_ID,
                _options.SLACK_APP_OAUTH_REDIRECT_URL,
                SLACK_APP_OAUTH_SCOPE,
                "state"));
        }
    }
}
