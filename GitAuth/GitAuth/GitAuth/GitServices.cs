using Octokit;
using System;
using System.Collections.Generic;
using System.IO;
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
        List<GithubCommit> coms = new List<GithubCommit>();

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
            Thread.Sleep(1000);
            List<IReadOnlyList<GitHubCommit>> commies = new List<IReadOnlyList<GitHubCommit>>();
            //List<GithubCommit> coms = new List<GithubCommit>();
            var repos = await client.Repository.GetAllForCurrent();

            List<string> fileExt = new List<string> { ".cs", ".py", ".java", ".go", ".js", ".ts", ".c", ".pl" };

            foreach (var repo in repos)
            {
                var commit = await client
                    .Repository
                    .Commit
                    .GetAll(repo.Owner.Login, repo.Name);


                foreach (var communist in commit)
                {
                    GithubCommit aCommit = new GithubCommit();
                    aCommit.sha = communist.Sha;
                    aCommit.author_name = communist.Commit.Author.Name;
                    aCommit.committer_name = communist.Commit.Committer.Name;
                    aCommit.author_date = communist.Commit.Author.Date;
                    // check date here and disregard old af stuff
                    if (communist.Commit.Author.Date.CompareTo(DateTimeOffset.Now.AddDays(-90)) < 1)
                    {
                        break;
                    }

                    var associated_files = await client.Repository.Commit.Get(repo.Id, communist.Sha);

                    foreach (var file in associated_files.Files)
                    {
                        aCommit.filename = file.Filename; // we can filter out unwanted files here
                        if (file.Filename.Contains(".") && !fileExt.Any(x => x.Contains(file.Filename.Substring(file.Filename.LastIndexOf('.')))))
                        {
                            break;
                        }

                        if (file.PreviousFileName != null)
                        {
                            aCommit.previous_file_name = file.PreviousFileName;
                        }
                        aCommit.blob_url = file.BlobUrl;
                        aCommit.raw_url = file.RawUrl; // we need this when we pickout dates, for now just grab all files

                        var request = (HttpWebRequest)WebRequest.Create(file.RawUrl);
                        var response = (HttpWebResponse)request.GetResponse();
                        aCommit.content = new StreamReader(response.GetResponseStream()).ReadToEnd();

                        aCommit.patch = file.Patch; // the changes

                        coms.Add(aCommit);
                    }
                }
                // download the file at currentdatetime - 90 days
            }
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
