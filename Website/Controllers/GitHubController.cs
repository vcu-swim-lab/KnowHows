using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;

namespace Website.Controllers
{
    public class GitHubController : ApiController
    {
        private const string GITHUB_APP_CLIENT_ID = "";
        private const string GITHUB_APP_CLIENT_SECRET = "";
        private const string GITHUB_APP_CLIENT_SCOPE = "user notifications repo";

        private const string GITHUB_APP_OAUTH_URL = "https://github.com/login/oauth/authorize";
        private const string GITHUB_APP_OAUTH_REDIRECT_URL = "https://github.com/login/oauth/authorize";

        public IHttpActionResult Authenticate(string bearer, string scope, string access_token)
        {
            Console.WriteLine("Received GitHub OAuth: {0}, {1}, {2}", bearer, scope, access_token);
            return Ok();
        }

        public string GetOAuthURL()
        {
            return GITHUB_APP_OAUTH_URL +
                String.Format("?client_id={0}&redirect_uri={1}&scope={2}&state={3}&allow_signup={4}",
                GITHUB_APP_CLIENT_ID,
                GITHUB_APP_OAUTH_REDIRECT_URL,
                GITHUB_APP_CLIENT_SCOPE,
                "state",
                "true");
        }
    }
}
