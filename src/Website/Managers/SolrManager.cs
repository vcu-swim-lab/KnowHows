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
using LibGit2Sharp;

namespace Website.Managers
{
    public class SolrManager
    {
        private const int RESULTS = 10;

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

        /// <summary>
        /// Return the top <see cref="RESULTS" /> results for a given basic query in a channel.
        /// </summary>
        public List<CodeDoc> BasicQuery(string search, string channelId)
        {
            ISolrOperations<CodeDoc> solr = ServiceLocator.Current.GetInstance<ISolrOperations<CodeDoc>>();

            var opts = new QueryOptions();
            var lang = GetLanguageRequest(search);
            if (!string.IsNullOrEmpty(lang))
                opts.AddFilterQueries(new SolrQueryByField("prog_language", lang));
            opts.AddFilterQueries(new SolrQueryByField("channel", channelId));
            opts.Rows = RESULTS;

            var query = new LocalParams { { "type", "boost" }, { "b", "recip(ms(NOW,author_date),3.16e-11,-1,1)" } } + new SolrQuery("unindexed_patch:\"" + search + "\"");
            var codeQuery = solr.Query(query, opts);

            List<CodeDoc> results = new List<CodeDoc>();
            foreach (CodeDoc doc in codeQuery) results.Add(doc);

            return results;
        }

        /// <summary>
        /// Return the top <see cref="RESULTS" /> results for a given natural language query in a channel.
        /// </summary>
        public List<CodeDoc> NaturalLangQuery(string search, string channelId)
        {
            ISolrOperations<CodeDoc> solr = ServiceLocator.Current.GetInstance<ISolrOperations<CodeDoc>>();

            var opts = new QueryOptions();
            var lang = GetLanguageRequest(search);
            if (!string.IsNullOrEmpty(lang))
                opts.AddFilterQueries(new SolrQueryByField("prog_language", lang));
            opts.AddFilterQueries(new SolrQueryByField("channel", channelId));
            opts.Rows = RESULTS;

            var query = new LocalParams { { "type", "boost" }, { "b", "recip(ms(NOW,author_date),3.16e-11,-1,1)" } } + (new SolrQuery("patch:\""+  search+ "\"") || new SolrQuery("commit_message:\"" + search + "\""));
            var codeQuery = solr.Query(query, opts);

            List<CodeDoc> results = new List<CodeDoc>();
            foreach (CodeDoc doc in codeQuery) results.Add(doc);

            return results;
        }

        /// <summary>
        /// Determine the language modifier in a query.
        /// </summary>
        private string GetLanguageRequest(string search)
        {
            // Capture /knowhows <...> in <language>
            var language_match = Regex.Match(search, " in (.+)").Groups;
            var lang = language_match[1].ToString();

            return lang;
        }

        /// <summary>
        /// Tracks a provided repository for a channel and adds its documents to Solr.
        /// </summary>
        public void TrackRepository(GitHubUser user, List<Octokit.Repository> repositories)
        {
            ISolrOperations<CodeDoc> solr = ServiceLocator.Current.GetInstance<ISolrOperations<CodeDoc>>();

            try
            {
                var clone_options = new CloneOptions();
                clone_options.CredentialsProvider = (_url, _user, _cred) => new UsernamePasswordCredentials { Username = user.GitHubAccessToken, Password = String.Empty};

                foreach (var repo in repositories)
                {
                    string repo_path = GetTemporaryDirectory();
                    string git_url = repo.CloneUrl;
                    LibGit2Sharp.Repository.Clone(git_url, repo_path, clone_options);

                    List<CodeDoc> commit_files = ParseRepository(user, repo_path);
                    solr.AddRange(commit_files);
                    solr.Commit();
                    Directory.Delete(repo_path, true);
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
                Console.WriteLine(String.Format("Error trying to track repository for {0}. Stopping...", user.UUID));
                Console.WriteLine(ex.ToString());
            }
            finally {
                solr.Commit();
            }
        }

        /// <summary>
        /// Untracks the provided repository from a channel and deletes its documents from Solr.
        /// </summary>
        public void UntrackRepository(GitHubUser user, List<String> repositories)
        {
            try
            {
                ISolrOperations<CodeDoc> solr = ServiceLocator.Current.GetInstance<ISolrOperations<CodeDoc>>();
                foreach (string repo in repositories)
                {
                    solr.Delete(new SolrQuery("channel:" + user.ChannelID) && new SolrQuery("repo:" + repo) && new SolrQuery("committer_name:" + user.UserID));
                }
                solr.Commit();
            }
            catch (Exception ex)
            {
                Console.WriteLine(String.Format("Error attemtping to untrack repositories for user {0}", user.UUID));
                Console.WriteLine(ex.ToString());
            }
        }

        /// <summary>
        /// Call GitParser on a local repository for parsing.
        /// The commit tree is walked until no new commits remain.
        /// </summary>
        private List<CodeDoc> ParseRepository(GitHubUser user, string repo_path)
        {
            List<CodeDoc> parsed_files = new List<CodeDoc>();
            GitParser git_parser = new GitParser(repo_path);

            do
            {
                var current_commit = git_parser.ParseCurrentCommit();
                foreach (CommitFile file in current_commit)
                {
                    parsed_files.Add(
                        new CodeDoc
                        {
                            Id = file.Commit_Sha,
                            Sha = file.Commit_Sha,
                            Author_Date = file.Authored_Date,
                            Author_Name = file.Author_Name,
                            Channel = user.ChannelID,
                            Committer_Name = user.UserID,
                            Accesstoken = user.GitHubAccessToken,
                            Filename = file.Filename,
                            Previous_File_Name = file.Previous_Filename,
                            Raw_Url = file.Raw_Url,
                            Blob_Url = file.Blob_Url,
                            Unindexed_Patch = file.Parsed_Patch,
                            Patch = file.Parsed_Patch,
                            Repo = file.Repository,
                            Html_Url = file.Commit_Url,
                            Message = file.Commit_Message,
                            Prog_Language = file.Language
                        }
                    );
                }
            } while (git_parser.Walk());

            return parsed_files;
        }

        /// <summary>
        /// Generate a path to a temporary directory.
        /// </summary>
        private string GetTemporaryDirectory()
        {
            string temp_folder = Path.GetTempFileName();
            Directory.CreateDirectory(temp_folder);
            return temp_folder;
        }
    }
}
