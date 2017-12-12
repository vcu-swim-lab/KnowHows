using System;
using System.Collections.Generic;
using Website.Manager;
using System.Linq;
using System.Net;
using Website.Utility.Solr;
using SolrNet;
using System.IO;
using CommonServiceLocator;
using SolrNet.Commands.Parameters;
using System.Threading.Tasks;

namespace Website.Managers
{
    public class SolrManager
    {
        string connection = "http://104.131.188.205:8983/solr/knowhows";
        public static SolrManager Instance = new SolrManager();

        private Dictionary<String, DateTime> lastFetchTimes;

        private SolrManager()
        {
            SolrNet.Startup.Init<CodeDoc>(connection);
            this.lastFetchTimes = new Dictionary<String, DateTime>();
        }

        public List<CodeDoc> PerformQuery(String query, String channelId)
        {
            return Query(query, channelId);
        }

        public void TrackRepository(GitHubUser user, String repository)
        {
            WebClient client = new WebClient();
            List<CodeDoc> result = new List<CodeDoc>();

            var repos = user.GitHubClient.Repository.GetAllForCurrent().Result;
            var repo = repos.Where(r => r.Name == repository).ToList()[0];
            var commits = user.GitHubClient.Repository.Commit.GetAll(repo.Owner.Login, repo.Name).Result;

            foreach (var commit in commits)
            {
                if (commit.Author.Login != repo.Owner.Login) continue;

                var associated_files = user.GitHubClient.Repository.Commit.Get(repo.Id, commit.Sha).Result;

                foreach (var file in associated_files.Files)
                {
                    CodeDoc doc = new CodeDoc();
                    doc.Sha = file.Sha;
                    doc.Author_Date = commit.Commit.Author.Date.Date;
                    doc.Author_Name = commit.Commit.Author.Name;
                    doc.Channel = user.ChannelID;
                    doc.Committer_Name = user.UserID;
                    doc.Accesstoken = user.GitHubAccessToken;
                    doc.Filename = file.Filename;
                    doc.Previous_File_Name = file.PreviousFileName;
                    doc.Id = user.ChannelID;          
                    doc.Content = client.DownloadString(new Uri(file.RawUrl));
                    doc.Raw_Url = file.RawUrl;
                    doc.Blob_Url = file.BlobUrl;
                    result.Add(doc);
                    Console.WriteLine("Adding {0} to Solr", doc.Filename);
                }
            }

            AddIndexed(result);
        }

        public void UnrackRepository(GitHubUser user, String repository)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// this is how we should be entering our data in, for searchability
        /// </summary>
        /// <param name="incoming"></param>
        public async Task AddIndexed(List<CodeDoc> incoming)
        {
            ISolrOperations<CodeDoc> solr = ServiceLocator.Current.GetInstance<ISolrOperations<CodeDoc>>();

            // this works better, can log bad responses if need be. response 1 == bad
            foreach (var inc in incoming)
            {
                var send = await solr.AddAsync(inc);
            }

            solr.Commit();
        }

        /// <summary>
        /// Provide a search string and filter string this will return the top result.  
        /// </summary>
        /// <param name="search">the search term</param>
        /// <param name="channelId">the channel to filter by</param>
        public List<CodeDoc> Query(string search, string channelId)
        {
            ISolrOperations<CodeDoc> solr = ServiceLocator.Current.GetInstance<ISolrOperations<CodeDoc>>();

            List<ISolrQuery> filter = new List<ISolrQuery>();
            filter.Add(new SolrQueryByField("channel", channelId));

            var opts = new QueryOptions();
            opts.ExtraParams = new KeyValuePair<string, string>[] {
                new KeyValuePair<string, string>("wt", "xml") // wt = writertype (response format)
            };

            // this should add an additional filter by channel ID 
            // this removes cross contamination
            foreach (var filt in filter)
            {
                opts.AddFilterQueries(filt);
            }

            var query = new SolrQuery(search);
            var codeQuery = solr.Query(query, opts);

            List<CodeDoc> results = new List<CodeDoc>();
            foreach (CodeDoc doc in codeQuery) results.Add(doc);

            return results;
        }

        /// <summary>
        /// getting weird errors but it sends the documents
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public async Task AddWithoutIndex(string filePath, string fileName)
        {
            ISolrOperations<CodeDoc> solr = ServiceLocator.Current.GetInstance<ISolrOperations<CodeDoc>>();

            using (var file = File.OpenRead(filePath))
            {
                var resp = await solr.ExtractAsync(new ExtractParameters(file, "main.go")
                {
                    ExtractOnly = false,
                    ExtractFormat = ExtractFormat.Text
                });
                solr.Commit();
            }
        }
    }
}
