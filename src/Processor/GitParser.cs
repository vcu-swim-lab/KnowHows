using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Net;
using LibGit2Sharp;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace Processor
{
    public class GitParser
    {

        private static readonly string RAW_URL_FORMAT = "https://raw.githubusercontent.com/{0}/{1}/{2}/{3}";
        private static readonly string BLOB_URL_FORMAT = "https://github.com/{0}/{1}/blob/{2}/{3}";
        private static readonly string HTML_URL_FORMAT = "https://github.com/{0}/{1}/commit/{2}";

        static readonly Regex REPO_URL_RE = new Regex(@"github\.com\/(.*)\/(.*)\.git");

        /// <summary>
        /// Parse a Git repository locally.
        /// TODO: Don't request patched files from GitHub, use the Git tree.
        /// </summary>
        public void ParseRepository()
        {
            using (var repo = new Repository(@"C:\Users\Alex\Desktop\KnowHows"))
            {
                Commit parent = repo.Head.Tip;
                IEnumerable<LibGit2Sharp.Commit> commits = repo.Commits.Skip(1);

                var repo_url = repo.Network.Remotes.First().Url;
                string repo_owner, repo_name;
                Match url_match = REPO_URL_RE.Match(repo_url);
                if (url_match.Success) {
                    repo_owner = url_match.Groups[1].Value;
                    repo_name = url_match.Groups[2].Value;
                }
                else {
                    throw new InvalidOperationException("Could not retrieve repository owner and name");
                }

                foreach (var commit in commits.Take(3))
                {
                    Console.WriteLine(String.Format("--- Parsing commit: {0}---", parent.Sha));
                    Commit child = commit;
                    Patch patch = repo.Diff.Compare<Patch>(child.Tree, parent.Tree);
                    foreach (var file_patch in patch)
                    {
                        var sha = parent.Sha;
                        var filename = file_patch.Path;

                        string ext = Path.GetExtension(filename);
                        if (String.IsNullOrEmpty(ext) || !SrcML.supportedExtensions.ContainsKey(ext))
                        {
                            Console.WriteLine("Skipping {0} ({1}): not supported by SrcML", filename, sha);
                            continue;
                        }

                        if (file_patch.LinesAdded == 0 || file_patch.Status == ChangeKind.Deleted)
                        {
                            Console.WriteLine("Skipping {0} ({1}): file was deleted", filename, sha);
                            continue;
                        }

                        var author_name = parent.Author.Name;
                        var author_date = parent.Author.When.Date;
                        var repository = repo_owner + "/" + repo_name;
                        var previous_file_name = file_patch.OldPath;
                        var raw_url = String.Format(RAW_URL_FORMAT, repo_owner, repo_name, sha, filename);
                        var blob_url = String.Format(BLOB_URL_FORMAT, repo_owner, repo_name, sha, filename);
                        var commit_url = String.Format(HTML_URL_FORMAT, repo_owner, repo_name, sha);
                        var unparsed_patch = file_patch.Patch;
                        var message = parent.Message;

                        var parsed_patch = FullyParsePatch(filename, raw_url, unparsed_patch);
                        if (String.IsNullOrEmpty(parsed_patch))
                        {
                            Console.WriteLine("Discarding {0} ({1}): no relevant terms found in parsed patch", filename, sha);
                            continue;
                        }

                        Console.WriteLine("Adding {0} ({1}) to Solr...", filename, sha);
                    }

                    parent = child;
                }
            }
        }

        /// <summary>
        /// Parse a unified diff to retrieve relevant terms for each file.
        /// </summary>
        private static string FullyParsePatch(string filename, string raw_url, string patch)
        {
            DiffParser parser = new DiffParser();
            string raw_file = GetFullRawFile(raw_url);
            var terms = parser.FindTerms(filename, raw_file, patch);
            return string.Join(" ", terms);
        }

        /// <summary>
        /// Get the raw file from GitHub.
        /// TODO: Don't request from GitHub, use the Git tree.
        /// </summary>
        private static string GetFullRawFile(string raw_url)
        {
            try
            {
                using (WebClient client = new WebClient())
                {
                    return client.DownloadString(raw_url);
                }
            }
            catch (WebException ex)
            {
                throw new InvalidOperationException("Failed to download full file: " + ex);
            }
        }
    }
}
