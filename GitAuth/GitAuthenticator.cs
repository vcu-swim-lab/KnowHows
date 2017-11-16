using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Runtime.InteropServices;
using Octokit;

namespace GitAuth
{
    public class GitAuthenticator
    {

        string clientID = Environment.GetEnvironmentVariable("clientid");
        string clientSecret = Environment.GetEnvironmentVariable("clientsecret");
        readonly GitHubClient client =
            new GitHubClient(new ProductHeaderValue("seniordesign2017wooooo"));
        private string _token;

        public GitAuthenticator()
        {
        }


        public async Task<string> oAuth()
        {
            string state = RandomURLKG(32);

            string redirectURL = "http://localhost:58292/"; // this apparently needs to be explicitly defined on GitHub site
            write("Redirect URL: " + redirectURL);

            // listens for redirection on the above url
            var http = new HttpListener();
            http.Prefixes.Add(redirectURL);
            write("who's talking?...");
            http.Start();

            string authorizationRequest = GetOauthLoginUrl(state);

            // starts it all up
            System.Diagnostics.Process.Start(authorizationRequest);

            // waiting for OAuth response
            var context = await http.GetContextAsync();

            // send em back to the hub
            var response = context.Response;
            string responseString = string.Format("<html><head><meta http-equiv='refresh' content='10;url=https://github.com'></head><body>Please return to the app.</body></html>");
            var buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
            response.ContentLength64 = buffer.Length;
            var responseOutput = response.OutputStream;
            Task responseTask = responseOutput.WriteAsync(buffer, 0, buffer.Length).ContinueWith((task) =>
            {
                responseOutput.Close();
                http.Stop();
                Console.WriteLine("server stopped.");
            });

            // checks for error
            if (context.Request.QueryString.Get("error") != null)
            {
                write(String.Format("OAuth authorization error: {0}.", context.Request.QueryString.Get("error")));
                return "";
            }
            if (context.Request.QueryString.Get("code") == null
                || context.Request.QueryString.Get("state") == null)
            {
                write("bad authorization response. " + context.Request.QueryString);
                return "";
            }

            // a precious code
            var code = context.Request.QueryString.Get("code");
            var incoming_state = context.Request.QueryString.Get("state");

            // state is our way of assuring we're getting who we think we are
            if (incoming_state != state)
            {
                write(String.Format("Received request with invalid state ({0})", incoming_state));
                return "";
            }
            write("Authorization code: " + code);
            _token = code;

            return code;
        }

        /// <summary>
        /// Unused
        /// </summary>
        /// <param name="code"></param>
        async void Authorize(string code)
        {
            if (!String.IsNullOrEmpty(code))
            {
                var token = await client.Oauth.CreateAccessToken(
                    new OauthTokenRequest(clientID, clientSecret, code));
                string clientToken = token.AccessToken; // from here pass token to DB? 
                client.Credentials = new Credentials(clientToken);

                // just pulling repos for testitng
                var repos = await client.Repository.GetAllForCurrent();
                foreach (var repo in repos)
                {
                    write(repo.Url);
                }
            }
        }

        /// <summary>
        /// Gets the URL for GitHub Authorization
        /// </summary>
        /// <param name="state"></param>
        /// <returns>the login url</returns>
        private string GetOauthLoginUrl(string state)
        {
            var request = new OauthLoginRequest(clientID)
            {
                Scopes = { "user", "notifications", "repo" },
                State = state
            };
            var oauthLoginUrl = client.Oauth.GetGitHubLoginUrl(request);
            return oauthLoginUrl.ToString();
        }

        /// <summary>
        /// Gets a free port, mostly for testing on return URLs, we can remove this
        /// </summary>
        /// <returns>port number</returns>
        public int UnusedPort()
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            var port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }

        /// <summary>
        /// Writes to screen
        /// </summary>
        /// <param name="write">string tot write</param>
        public void write(string write)
        {
            Console.WriteLine(write);
            //Logging.Log.Content.Trace(write);
        }

        /// <summary>
        /// Will generate a key  
        /// </summary>
        /// <param name="length">input length</param>
        /// <returns></returns>
        public string RandomURLKG(uint length)
        {
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            byte[] bytes = new byte[length];
            rng.GetBytes(bytes);
            return Encoder(bytes);
        }

        /// <summary>
        /// Encodes a given buffer for key creation
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public string Encoder(byte[] buffer)
        {
            string base64 = Convert.ToBase64String(buffer);

            // makes sure it works in html
            base64 = base64.Replace("+", "-");
            base64 = base64.Replace("/", "_");
            base64 = base64.Replace("=", "");

            return base64;
        }
    }
}
