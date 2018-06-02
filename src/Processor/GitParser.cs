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
        private static readonly Regex REPO_URL_RE = new Regex(@"github\.com\/(.*)\/(.*)\.git");

        private Repository repository;
        private Commit head, child;
        private string repo_owner, repo_name;

        public GitParser(string repo_path) {
            repository = new Repository(@repo_path);
            head = repository.Head.Tip;
            child = GetNextChildCommit(head);

            // Determine the repository owner and name on GitHub from the remote URL
            var repo_url = repository.Network.Remotes.First().Url;
            Match url_match = REPO_URL_RE.Match(repo_url);
            if (url_match.Success) {
                repo_owner = url_match.Groups[1].Value;
                repo_name = url_match.Groups[2].Value;
            }
            else {
                throw new InvalidOperationException(String.Format("Could not retrieve repository owner and name for {0}", repo_path));
            }
        }

        /// <summary>
        /// Parse the commit pointed to from the current HEAD.
        /// Use Walk() to move the HEAD pointer down the commit tree and retrieve successive commits.
        /// Returns a list of CommitFile objects that can be inserted in Solr.
        /// </summary>
        public List<CommitFile> ParseCurrentCommit()
        {
            List<CommitFile> commit_files = new List<CommitFile>();

            Patch patch;
            if (child != null)
            {
                patch = repository.Diff.Compare<Patch>(child.Tree, head.Tree);
            }
            else {
                return commit_files;
            }

            var commit_sha = head.Sha;
            string author_name = head.Author.Name;
            string authored_date = head.Author.When.Date.ToString(); // The local commit time, so it'll differ from the GitHub commit date
            string repository_name = this.repo_owner + "/" + this.repo_name;
            string commit_message = head.Message;

            Console.WriteLine(String.Format("Parsing commit: {0}/{1}", repository_name, commit_sha));

            foreach (var file_patch in patch)
            {
                var filename = file_patch.Path;
                string ext = Path.GetExtension(filename);
                if (String.IsNullOrEmpty(ext) || !SrcML.supportedExtensions.ContainsKey(ext))
                {
                    Console.WriteLine("Skipping {0} ({1}): not supported by SrcML", filename, commit_sha);
                    continue;
                }
                if (file_patch.LinesAdded == 0 || file_patch.Status == ChangeKind.Deleted)
                {
                    Console.WriteLine("Skipping {0} ({1}): file was deleted", filename, commit_sha);
                    continue;
                }
                if (head[filename] == null) {
                    throw new NullReferenceException(String.Format("Could not retrieve {0} from current commit tree ({1}", filename, commit_sha));
                }

                string previous_filename = file_patch.OldPath;
                string raw_url = String.Format(RAW_URL_FORMAT, this.repo_owner, this.repo_name, commit_sha, filename);
                string blob_url = String.Format(BLOB_URL_FORMAT, this.repo_owner, this.repo_name, commit_sha, filename);
                string commit_url = String.Format(HTML_URL_FORMAT, this.repo_owner, this.repo_name, commit_sha);

                string parsed_patch = FullyParsePatch(head, filename, file_patch.Patch);
                if (String.IsNullOrEmpty(parsed_patch))
                {
                    Console.WriteLine("Discarding {0} ({1}): no relevant terms found in parsed patch", filename, commit_sha);
                    continue;
                }

                commit_files.Add(
                    new CommitFile
                    {
                        Commit_Sha = commit_sha,
                        Filename = filename,
                        Language = SrcML.supportedExtensions[ext],
                        Previous_Filename = previous_filename,
                        Author_Name = author_name,
                        Authored_Date = DateTime.Parse(authored_date),
                        Repository = repository_name,
                        Raw_Url = raw_url,
                        Blob_Url = blob_url,
                        Commit_Url = commit_url,
                        Commit_Message = commit_message,
                        Parsed_Patch = parsed_patch
                    }
                );
            }

            return commit_files;
        }

        /// <summary>
        /// Walk the HEAD and child pointers to the next commit.
        /// Returns false when no children are left from the current HEAD, indicating
        /// the end of the branch.
        /// </summary>
        public bool Walk() {
            Commit next_child = GetNextChildCommit(child);
            if (next_child != null) {
                head = child;
                child = next_child;
                return true;
            }
            else {
                return false;
            }
        }

        /// <summary>
        /// Retrieve the next child commit from the provided commit.
        /// If there is no child commit, return null.
        /// </summary>
        private Commit GetNextChildCommit(Commit commit_pointer) {
            if (commit_pointer == null) return null;
            var filter = new CommitFilter { IncludeReachableFrom = commit_pointer };
            var commit_log = repository.Commits.QueryBy(filter).Skip(1);
            return commit_log.Any() ? commit_log.First() : null;
        }

        /// <summary>
        /// Parse a unified diff to retrieve relevant terms for each file.
        /// Returns a string of terms joined by spaces.
        /// </summary>
        private static string FullyParsePatch(Commit current_commit, string filename, string patch)
        {
            DiffParser parser = new DiffParser();
            string raw_file = GetFullRawFile(current_commit, filename);
            var terms = parser.FindTerms(filename, raw_file, patch);
            return string.Join(" ", terms);
        }

        /// <summary>
        /// Get the raw patched file for the current commit.
        /// </summary>
        private static string GetFullRawFile(Commit current_commit, String filename)
        {
            var raw_file = current_commit[filename];
            var raw_file_blob = (Blob)raw_file.Target;
            var raw_content = raw_file_blob.GetContentStream();
            using (var raw_reader = new StreamReader(raw_content, Encoding.UTF8))
            {
                return raw_reader.ReadToEnd();
            }
        }
    }
}
