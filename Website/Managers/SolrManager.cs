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

        private static String[] validExtensions = new String[] { ".cs", ".java", ".c", ".cpp", ".h", ".py", ".js" };
        public void TrackRepository(GitHubUser user, String repository)
        {
            //WebClient client = new WebClient();
            List<CodeDoc> result = new List<CodeDoc>();

            ISolrOperations<CodeDoc> solr = ServiceLocator.Current.GetInstance<ISolrOperations<CodeDoc>>();
            var repos = user.GitHubClient.Repository.GetAllForCurrent().Result;
            var repo = repos.Where(r => r.Name == repository).ToList()[0];
            var commits = user.GitHubClient.Repository.Commit.GetAll(repo.Owner.Login, repo.Name).Result;

            foreach (var commit in commits)
            {
                if (commit.Author != null && commit.Author.Login != user.GitHubClient.User.Current().Result.Login) continue;

                var associated_files = user.GitHubClient.Repository.Commit.Get(repo.Id, commit.Sha).Result;
                foreach (var file in associated_files.Files)
                {
                    if (!validExtensions.Contains(Path.GetExtension(file.Filename))) continue;

                    CodeDoc doc = new CodeDoc();
                    doc.Id = file.Sha;
                    doc.Sha = file.Sha;
                    doc.Author_Date = commit.Commit.Author.Date.Date;
                    doc.Author_Name = commit.Commit.Author.Name;
                    doc.Channel = user.ChannelID;
                    doc.Committer_Name = user.UserID;
                    doc.Accesstoken = user.GitHubAccessToken;
                    doc.Filename = file.Filename;
                    doc.Previous_File_Name = file.PreviousFileName;                        
                    doc.Raw_Url = file.RawUrl;
                    doc.Blob_Url = file.BlobUrl;
                    // doc.Content = client.DownloadString(new Uri(file.RawUrl));    
                    doc.Patch = file.Patch;
                    doc.Repo = repo.Name;

                    solr.Add(doc);
                    Console.WriteLine("Adding {0} to Solr", doc.Filename);
                }
            }

            solr.Commit();
            Console.WriteLine("Finished tracking repository {0} for {1} to Solr", repository, user.UUID);
        }

        /// <summary>
        /// removes a given user's repo from a channel
        /// </summary>
        /// <param name="user"></param>
        /// <param name="channel"></param>
        /// <param name="repository"></param>
        public void UntrackRepository(GitHubUser user, String channel, String repository)
        {
            ISolrOperations<CodeDoc> solr = ServiceLocator.Current.GetInstance<ISolrOperations<CodeDoc>>();

            solr.Delete( new SolrQuery("channel:" + channel) && new SolrQuery("repo:" + repository) && new SolrQuery("committer_name:"+user.UserID));

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
            var opts = new QueryOptions();

            filter.Add(new SolrQueryByField("channel", channelId));        
            foreach (var filt in filter) opts.AddFilterQueries(filt);

            var query = new SolrQuery("text:\"" + search + "\"");
            var codeQuery = solr.Query(query, opts);

            List<CodeDoc> results = new List<CodeDoc>();
            foreach (CodeDoc doc in codeQuery) results.Add(doc);

            return results;
        }
    }
}
