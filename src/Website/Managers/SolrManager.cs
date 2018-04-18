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
using System.Net;
using Octokit;

namespace Website.Managers
{
    public class SolrManager
    {
        private const int RESULTS = 10;
        private const int COMMITS_THRESHOLD = 50;

        ISolrConnection connection = new SolrNet.Impl.SolrConnection(SolrUrl.SOLR_URL)
        {
            HttpWebRequestFactory = new HttpWebAdapters.BasicAuthHttpWebRequestFactory(SolrUrl.SOLR_USER, SolrUrl.SOLR_SECRET)
        };

        public static SolrManager Instance = new SolrManager();

        private Dictionary<String, DateTime> lastFetchTimes;

        private SolrManager()
        {
            SolrNet.Startup.Init<CodeDoc>(connection);
            this.lastFetchTimes = new Dictionary<String, DateTime>();
        }

        public List<CodeDoc> PerformNLPQuery(String query, String channelId)
        {
            return NaturalLangQuery(query, channelId);
        }

        public List<CodeDoc> PerformQuery(String query, String channelId)
        {
            return BasicQuery(query, channelId);
        }

        private List<CodeDoc> GetDocumentsFromCommit(GitHubUser user, Repository repo, GitHubCommit commit)
        {
            List<CodeDoc> cd = new List<CodeDoc>();
            var associated_files = user.GitHubClient.Repository.Commit.Get(repo.Id, commit.Sha).Result.Files;

            foreach (var file in associated_files)
            {
                string ext = Path.GetExtension(file.Filename);

                if (String.IsNullOrEmpty(ext) || !SrcML.supportedExtensions.ContainsKey(ext))
                {
                    Console.WriteLine("Skipping {0} ({1}): not supported by SrcML", file.Filename, file.Sha);
                    continue;
                }

                if (file.Additions == 0 || String.Equals(file.Status, "removed"))
                {
                    Console.WriteLine("Skipping {0} ({1}): file was deleted", file.Filename, file.Sha);
                    continue;
                }

                // pulls out relevant information for later searching
                string parsedPatch = FullyParsePatch(file.Filename, file.RawUrl, file.Patch);

                if (String.IsNullOrEmpty(parsedPatch))
                {
                    Console.WriteLine("Discarding {0} ({1}): no relevant terms found in parsed patch", file.Filename, file.Sha);
                    continue;
                }

                CodeDoc doc = new CodeDoc
                {
                    Id = file.Sha,
                    Sha = file.Sha,
                    Author_Date = commit.Commit.Author.Date.Date,
                    Author_Name = commit.Commit.Author.Name,
                    Channel = user.ChannelID,
                    Committer_Name = user.UserID,
                    Accesstoken = user.GitHubAccessToken,
                    Filename = file.Filename,
                    Previous_File_Name = file.PreviousFileName,
                    Raw_Url = file.RawUrl,
                    Blob_Url = file.BlobUrl,
                    Unindexed_Patch = parsedPatch,
                    Patch = parsedPatch,
                    Repo = repo.Name,
                    Html_Url = commit.HtmlUrl,
                    Message = commit.Commit.Message,
                    Prog_Language = SrcML.supportedExtensions[ext]
                };

                cd.Add(doc);
                Console.WriteLine("Adding {0}/{1} ({2}) to Solr", doc.Repo, doc.Filename, doc.Sha);
            }

            return cd;
        }

        public void TrackRepository(GitHubUser user, List<Repository> repositories)
        {
            ISolrOperations<CodeDoc> solr = ServiceLocator.Current.GetInstance<ISolrOperations<CodeDoc>>();

            try {
                foreach (var repo in repositories)
                {
                    var commits_processed = 0;
                    var commits = user.GitHubClient.Repository.Commit.GetAll(repo.Owner.Login, repo.Name).Result;

                    foreach (var commit in commits)
                    {
                        if (commits_processed > COMMITS_THRESHOLD) {
                            Console.WriteLine(String.Format("Commits threshold reached on {0} for {1}. Stopping...", repo.Name, user.UUID));
                            break;
                        }

                        if (commit.Author != null && commit.Author.Login != user.GitHubClient.User.Current().Result.Login) continue;
                        var docsToAdd = GetDocumentsFromCommit(user, repo, commit);

                        foreach (var doc in docsToAdd) solr.Add(doc);

                        commits_processed++;
                    }

                    solr.Commit();
                    Console.WriteLine(String.Format("Finished tracking {0} for {1} to Solr", repo.Name , user.UUID));
                }
            }
            catch (RateLimitExceededException) {
                Console.WriteLine(String.Format("Rate limit exceeded for {0}. Stopping...", user.UUID));
            }
            catch (AbuseException) {
                Console.WriteLine(String.Format("Abuse detection triggered for {0}. Stopping...", user.UUID));
            }
            catch (Exception ex) {
                Console.WriteLine(String.Format("Error trying to retrieve commits for {0}. Stopping...", user.UUID));
                Console.WriteLine(ex.ToString());
            }
            finally {
                solr.Commit();
            }
        }

        private string FullyParsePatch(string fileName, string rawUrl, string patch)
        {
            DiffParser parser = new DiffParser();
            string rawFile = GetFullRawFile(rawUrl);
            var terms = parser.FindTerms(fileName, rawFile, patch);

            return string.Join(' ', terms);
        }

        private string GetFullRawFile(string rawUrl)
        {
            string fullFile = "";
            try
            {
                using (WebClient cl = new WebClient())
                {
                    fullFile = cl.DownloadString(rawUrl);
                }
            }
            catch (WebException ex)
            {
                Console.WriteLine("Failed to download a full file: " + ex);
            }
            return fullFile;
        }

        /// <summary>
        /// removes a given user's repo from a channel
        /// </summary>
        /// <param name="user"></param>
        /// <param name="channel"></param>
        /// <param name="repository"></param>
        public void UntrackRepository(GitHubUser user, List<String> repository)
        {
            try
            {
                ISolrOperations<CodeDoc> solr = ServiceLocator.Current.GetInstance<ISolrOperations<CodeDoc>>();

                if(repository.Count > 1)
                    solr.Delete(new SolrQuery("channel:" + user.ChannelID) && new SolrQuery("repo:" + repository[0]) && new SolrQuery("committer_name:" + user.UserID));
               else
                    solr.Delete(new SolrQuery("channel:" + user.ChannelID) && new SolrQuery("committer_name:" + user.UserID));
                solr.Commit();
            }
            catch(Exception ex) { Console.WriteLine(ex.ToString()); }
        }

        /// <summary>
        /// Queries Solr and looks for exact matches 
        /// </summary>
        /// <param name="search"></param>
        /// <param name="channelId"></param>
        /// <returns></returns>
        private List<CodeDoc> BasicQuery(string search, string channelId)
        {
            ISolrOperations<CodeDoc> solr = ServiceLocator.Current.GetInstance<ISolrOperations<CodeDoc>>();

            List<ISolrQuery> filter = new List<ISolrQuery>();
            var opts = new QueryOptions();

            var lang = GetLanguageRequest(search);

            if (!string.IsNullOrEmpty(lang))
                filter.Add(new SolrQueryByField("prog_language", lang));

            filter.Add(new SolrQueryByField("channel", channelId));
            foreach (var filt in filter) opts.AddFilterQueries(filt);

            // return top n results
            opts.Rows = RESULTS;

            var query = new LocalParams { { "type", "boost" }, { "b", "recip(ms(NOW,author_date),3.16e-11,-1,1)" } } + new SolrQuery("unindexed_patch:\"" + search + "\"");
            var codeQuery = solr.Query(query, opts);

            List<CodeDoc> results = new List<CodeDoc>();
            foreach (CodeDoc doc in codeQuery) results.Add(doc);

            return results;
        }

        /// <summary>
        /// Provide a search string and filter string this will return the top n results.  
        /// </summary>
        /// <param name="search">the search term</param>
        /// <param name="channelId">the channel to filter by</param>
        public List<CodeDoc> NaturalLangQuery(string search, string channelId)
        {
            // there is some duplication, should be cleaned up 
            ISolrOperations<CodeDoc> solr = ServiceLocator.Current.GetInstance<ISolrOperations<CodeDoc>>();

            List<ISolrQuery> filter = new List<ISolrQuery>();
            var opts = new QueryOptions();

            var lang = GetLanguageRequest(search);

            if (!string.IsNullOrEmpty(lang))
                filter.Add(new SolrQueryByField("prog_language", lang));


            filter.Add(new SolrQueryByField("channel", channelId));
            foreach (var filt in filter) opts.AddFilterQueries(filt);
            
            // return top n results 
            opts.Rows = RESULTS;

            var query = new LocalParams { { "type", "boost" }, { "b", "recip(ms(NOW,author_date),3.16e-11,-1,1)" } } + (new SolrQuery("patch:"+  search) || new SolrQuery("commit_message:" + search));
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
