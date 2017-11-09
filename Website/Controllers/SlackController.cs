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

        public class SlashCommand
        {
            public string token, team_id, team_domain, enterprise_id, enterprise_name, channel_id, channel_name, user_id, user_name, command, text, response_url, trigger_id;
            public SlashCommand(FormDataCollection data)
            {
                this.token              = data.Get("token");
                this.team_id            = data.Get("team_id");
                this.team_domain        = data.Get("team_domain");
                this.enterprise_id      = data.Get("enterprise_id");
                this.enterprise_name    = data.Get("enterprise_name");
                this.channel_id         = data.Get("channel_id");
                this.channel_name       = data.Get("channel_name");
                this.user_id            = data.Get("user_id");
                this.user_name          = data.Get("user_name");
                this.command            = data.Get("command");
                this.text               = data.Get("text");
                this.response_url       = data.Get("response_url");
                this.trigger_id         = data.Get("trigger_id");
            }
        }

        private class OAuthRequest
        {
            public string client_id, client_secret, code, redirect_uri;
        }

        private class OAuthResponse
        {
            public string access_token, scope;
        }

        public IHttpActionResult ProcessMessage(FormDataCollection data)
        {
            // @TODO: there should be a better way of deserializing this
            SlashCommand command = new SlashCommand(data);

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
            Console.WriteLine("Received access token for code {0}, access token: {1}", code, response.access_token);

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
    }
}
