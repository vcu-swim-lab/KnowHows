using Octokit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GitAuth
{
    public class GitServices
    {
        string _code;
        string clientID = Environment.GetEnvironmentVariable("clientid");
        string clientSecret = Environment.GetEnvironmentVariable("clientsecret");
        public readonly GitHubClient client =
            new GitHubClient(new ProductHeaderValue("seniordesign2017wooooo"));
        private Task<GitHubClient> task;
        private Queue<string> gitData = new Queue<string>();

        OauthToken token;

        public GitServices(string toke)
        {
            _code = toke;
            GenerateClient();
        }

        public async void GenerateClient()
        {
            token = await client.Oauth.CreateAccessToken(
                new OauthTokenRequest(clientID, clientSecret, _code));

            string clientToken = token.AccessToken;
            client.Credentials = new Credentials(clientToken);
        }

        public GitServices(Task<GitHubClient> task)
        {
            this.task = task;
        }

        public async Task GitCommits(string code)
        {
            var collectCommits = await client.Repository.Commit.Get("vcu-swim-lab", "HelpMeCode", "be3daca02b00447742d63fbf4c421d266319f116");

            var files = collectCommits.Files;
        }

        /// <summary>
        /// Gets all files from current authenticated repos. 
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public async Task GitFiles(string code)
        {
            Thread.Sleep(1000);
            var repos = await client.Repository.GetAllForCurrent();

            List<string> fileExt = new List<string> { ".cs", ".py", ".java", ".go", ".js" };

            foreach (var repo in repos)
            {
                var file = await client
                    .Repository
                    .Content
                    .GetAllContentsByRef(repo.Id, "master");

                var toSend = file.ToList();
                toSend.ForEach(x => gitData.Enqueue(x.DownloadUrl));
            }
            while (gitData.Any())
            {
                DownloadData();
            }

        }

        /// <summary>
        /// Ideally we should be sending the stream either to our parsing algo or directly to Solr
        /// </summary>
        private void DownloadData()
        {
            if (gitData.Any())
            {
                WebClient webby = new WebClient();
                webby.DownloadFileCompleted += Webby_DownloadFileCompleted;

                var currentUrl = gitData.Dequeue();
                if (currentUrl != null)
                {
                    string fileName = currentUrl.Substring(currentUrl.LastIndexOf('/') + 1, currentUrl.Length - currentUrl.LastIndexOf('/') - 1);
                    try
                    {
                        // set path either to DB or download into memory and pass to algo indexer                    
                        webby.DownloadFileTaskAsync(new Uri(currentUrl), "path" + fileName).Wait();
                    }
                    catch (InvalidOperationException e)
                    {
                        write("Bad file format for" + fileName);
                    }

                }
                return;
            }
        }

        private void Webby_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                write("error occurred in download...");
            }
            if (e.Cancelled)
            {
                write("cancelled download...");
            }
            DownloadData();
        }

        public async void GitCommiters()
        {
            var repos = await client.Repository.GetAllForCurrent();

            foreach (var repo in repos)
            {
                Console.Write(repo.Url);
            }
        }

        public void write(string write)
        {
            Console.WriteLine(write);
            //Logging.Log.Content.Trace(write);
        }

    }
}
