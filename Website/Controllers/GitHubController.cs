using System;
using System.Net;

using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

using Website.Utility;
using Website.Utility.OAuth;
using System.Collections.Generic;
using Website.Manager;

namespace Website.Controllers
{
    [Route("api/github")]
    public class GitHubController : Controller
    {
        private const string GITHUB_APP_OAUTH_URL = "https://github.com/login/oauth/authorize";
        private const string GITHUB_APP_OAUTH_ACCESS_URL = "https://github.com/login/oauth/access_token";
        private const string GITHUB_APP_OAUTH_SCOPE = "user notifications repo";

       

        private readonly AppSettings _options;
        public GitHubController(IOptions<AppSettings> optionsAccessor)
        {
            _options = optionsAccessor.Value;
        }

        private OAuthResponse RequestAccessToken(string code)
        {
            WebClient client = new WebClient();
            client.Headers["Accept"] = "application/json";

            string uri = GITHUB_APP_OAUTH_ACCESS_URL + String.Format("?client_id={0}&client_secret={1}&code={2}&redirect_uri={3}&state={4}",
                _options.GITHUB_APP_CLIENT_ID,
                _options.GITHUB_APP_CLIENT_SECRET,
                code,
                _options.GITHUB_APP_OAUTH_REDIRECT_URL,
                "state");

            string response = client.DownloadString(new Uri(uri));

            try { return JsonConvert.DeserializeObject<OAuthResponse>(response); }
            catch (Exception ex)
            {
                throw new Exception(String.Format("Failed to process GitHub OAuth Access Token response for code: '{0}', reason: {1}", code, ex.ToString()));
            }
        }

        [HttpGet]
        [Route("Authenticate")]
        public IActionResult Authenticate(OAuthRequest request)
        {
            Console.WriteLine("Received GitHub intall: '{0}', '{1}'", request.code, request.state);

            // @TODO: save the token and associate it with something
            OAuthResponse response = RequestAccessToken(request.code);
            Console.WriteLine("Received GitHub OAuth: '{0}', '{1}'", request.code, response.access_token);

            UserManager.Instance.AddGitHubAuth(request.state, response.access_token);

            return Ok("You may now close this tab");
        }

        [HttpGet]
        [Route("getOAuthURL")]
        public IActionResult GetOAuthURL(string uuid)
        {
            UserManager.Instance.AddPendingGitHubAuth(uuid);

            return Redirect(GITHUB_APP_OAUTH_URL +
                String.Format("?client_id={0}&redirect_uri={1}&scope={2}&state={3}&allow_signup={4}",
                _options.GITHUB_APP_CLIENT_ID,
                _options.GITHUB_APP_OAUTH_REDIRECT_URL,
                GITHUB_APP_OAUTH_SCOPE,
                uuid,
                "true"));
        }
    }
}
