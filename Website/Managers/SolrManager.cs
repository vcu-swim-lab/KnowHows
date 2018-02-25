using System;
using System.Collections.Generic;
using Website.Manager;
using System.Linq;
using Website.Utility.Solr;
using SolrNet;
using System.IO;
using CommonServiceLocator;
using SolrNet.Commands.Parameters;
using System.Text.RegularExpressions;
using Processor;

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
            List<CodeDoc> result = new List<CodeDoc>();
            Dictionary<string, string> languageDict = new Dictionary<string, string>();
            DiffParser parser = new DiffParser();

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
                    string ext = Path.GetExtension(file.Filename);
                    if (!SrcML.supportedExtensions.ContainsKey(ext))
                    {
                        Console.WriteLine("Skipping {0} ({1}): not supported by SrcML", file.Filename, file.Sha);
                        continue;
                    }

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
                    doc.Patch = file.Patch;
                    doc.Repo = repo.Name;
                    doc.Html_Url = commit.Commit.Url;
                    doc.Prog_Language = SrcML.supportedExtensions[ext];

                    solr.Add(doc);
                    Console.WriteLine("Adding {0} ({1}) to Solr", doc.Filename, doc.Sha);
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
        public void UntrackRepository(GitHubUser user, String repository)
        {
            ISolrOperations<CodeDoc> solr = ServiceLocator.Current.GetInstance<ISolrOperations<CodeDoc>>();

            solr.Delete(new SolrQuery("channel:" + user.ChannelID) && new SolrQuery("repo:" + repository) && new SolrQuery("committer_name:" + user.UserID));

            solr.Commit();
        }

        /// <summary>
        /// Provide a search string and filter string this will return the top 5 results.  
        /// </summary>
        /// <param name="search">the search term</param>
        /// <param name="channelId">the channel to filter by</param>
        public List<CodeDoc> Query(string search, string channelId)
        {
            ISolrOperations<CodeDoc> solr = ServiceLocator.Current.GetInstance<ISolrOperations<CodeDoc>>();

            List<ISolrQuery> filter = new List<ISolrQuery>();
            var opts = new QueryOptions();

            var lang = GetLanguageRequest(search);

            if (!string.IsNullOrEmpty(lang))
                filter.Add(new SolrQueryByField("prog_language", lang));

            filter.Add(new SolrQueryByField("channel", channelId));
            foreach (var filt in filter) opts.AddFilterQueries(filt);
            // return top 5 results
            opts.Rows = 5;

            var query = new SolrQuery("patch:\"" + search + "\"");
            var codeQuery = solr.Query(query, opts);

            List<CodeDoc> results = new List<CodeDoc>();
            foreach (CodeDoc doc in codeQuery) results.Add(doc);

            return results;
        }

        private string GetLanguageRequest(string search)
        {
            // capture the in "language", in group 2 
            search += " ";
            var inLanguage = Regex.Match(search, " (in) (.+) ").Groups;
            var lang = inLanguage[2].ToString();

            return lang;
        }
    }
}
