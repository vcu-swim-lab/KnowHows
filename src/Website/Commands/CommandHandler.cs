using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Website.Manager;
using Website.Managers;
using Website.Utility.Solr;

namespace Website.Commands
{
    public class CommandHandler
    {
        public static CommandResponse HandleCommand(GitHubUser user, Command command, bool autoRun = false)
        {
            try
            {
                if (autoRun)
                    return HandleAutoRunTracking(user);

                string action = command.text.Split(' ')[0];
                switch (action)
                {
                    case "search":  return HandleSearch(user, command);
                    case "track":   return HandleTrack(user, command);
                    case "untrack": return HandleUntrack(user, command);
                    case "help":    return HandleHelp(user, command);
                    case "to":      return HandleNaturalLanguageSearch(user, command);
                }
                return HandleHelp(user, command);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return new CommandResponse(String.Format("There was an *Error* handling that request, please try again."));
            }       
        }

        private static CommandResponse HandleAutoRunTracking(GitHubUser user)
        {
            if (user.AutoTrackRepos())
                return new CommandResponse("Thanks for supporting us! :heart_eyes: All of your public repositories were automatically tracked. To get help with available commands, please use `/knowhows help`.");
            return new CommandResponse("Thanks for supporting us! :heart_eyes: To get help with available commands, please type `/knowhows help`.");
        }

        private static CommandResponse HandleNaturalLanguageSearch(GitHubUser user, Command command)
        {
            try
            {
                string query = ObtainQuery(command.text);
                if (String.IsNullOrEmpty(query)) return new CommandResponse("*No results found*: empty query was provided");

                var results = SolrManager.Instance.PerformNLPQuery(query, user.ChannelID);
                StringBuilder sb = new StringBuilder();

                if (results.Count == 0) sb.AppendLine("*No results found*");
                else
                {
                    if (results.Count == 1) sb.AppendLine(String.Format("Found *{0}* result for *{1}*:", results.Count, query));
                    else sb.AppendLine(String.Format("Found *{0}* results for *{1}*:", results.Count, query));
                    sb.AppendLine(GenerateResults(results));
                }

                return new CommandResponse(sb.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return new CommandResponse(":( There was an error processing your request, please try again.");
            }
        }

        private static CommandResponse HandleSearch(GitHubUser user, Command command)
        {
            try
            {
                string query = ObtainQuery(command.text);
                if (String.IsNullOrEmpty(query)) return new CommandResponse("*No results found*: empty query was provided");

                var results = SolrManager.Instance.PerformQuery(query, user.ChannelID);
                StringBuilder sb = new StringBuilder();

                if (results.Count == 0) sb.AppendLine("*No results found*");
                else
                {
                    if (results.Count == 1) sb.AppendLine(String.Format("Found *{0}* result for *{1}*:", results.Count, query));
                    else sb.AppendLine(String.Format("Found *{0}* results for *{1}*:", results.Count, query));
                    sb.AppendLine(GenerateResults(results));
                }

                return new CommandResponse(sb.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return new CommandResponse(":( There was an error processing your request, please try again.");
            }
        }

        private static string GenerateResults(List<CodeDoc> results)
        {
            StringBuilder sb = new StringBuilder();

            while (results.Any())
            {
                var topUser = results[0].Committer_Name;

                // begining of new result
                sb.Append(String.Format("• *<@{0}>* knows and made changes to <{2}|*{3}/{1}*> ({4})",
                    results[0].Committer_Name,
                    results[0].Filename,
                    results[0].Html_Url,
                    results[0].Repo,
                    results[0].Sha.Substring(0, 5)));

                var allTopUserResults = results.Where(x => x.Committer_Name == topUser).ToList();

                if (results.Count > 1)
                {
                    for (int i = 1; i < allTopUserResults.Count; i++)
                    {
                        // last result
                        if (i == allTopUserResults.Count - 1)
                        {
                            sb.Append(String.Format(" and <{1}|*{2}/{0}*> ({3}).",
                                allTopUserResults[i].Filename,
                                allTopUserResults[i].Html_Url,
                                allTopUserResults[i].Repo,
                                allTopUserResults[i].Sha.Substring(0, 5)));
                        }
                        else
                            sb.Append(String.Format(", <{1}|*{2}/{0}*> ({3}),",
                                allTopUserResults[i].Filename,
                                allTopUserResults[i].Html_Url,
                                allTopUserResults[i].Repo,
                                allTopUserResults[i].Sha.Substring(0, 5)));
                    }
                }

                results.RemoveAll(x => x.Committer_Name == topUser);
                sb.AppendLine("");
            }

            return sb.ToString();
        }

        private static string ObtainQuery(string text)
        {
            var query_terms = text.Split(new[]{' '}, 2);
            return query_terms.Length > 1 ? query_terms[1] : null;
        }

        private static CommandResponse HandleTrack(GitHubUser user, Command command)
        {
            try
            {
                string text = command.text, action = text.Split(' ')[0];
                string repository = text.Split(' ').Length >= 2 ? command.text.Substring(text.IndexOf(action) + (action.Length + 1)) : "";

                if (!string.IsNullOrEmpty(repository) && user.TrackRepository(repository))
                {
                    if (repository == "*")
                        repository = "*all repositories*";
                    return new CommandResponse("*Successfully tracked* " + repository);
                }
                else
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("*Here are your available untracked repositories:* ");
                    sb.AppendLine(FormatRepositoryList(user.UntrackedRepositories));
                    return new CommandResponse(sb.ToString());
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return new CommandResponse(":cry: There was an error processing your request. Please try again.");
            }
        }

        private static CommandResponse HandleUntrack(GitHubUser user, Command command)
        {
            try
            {
                string text = command.text, action = text.Split(' ')[0];
                string repository = text.Split(' ').Length >= 2 ? command.text.Substring(text.IndexOf(action) + (action.Length + 1)) : "";

                if (user.UntrackRepository(repository))
                {
                    if (repository == "*")
                        repository = "*all repositories*";
                    return new CommandResponse("*Successfully untracked* " + repository);
                }
                else
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("*Here are your available tracked repositories:* ");
                    sb.AppendLine(FormatRepositoryList(user.TrackedRepositories));
                    return new CommandResponse(sb.ToString());
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return new CommandResponse(":( There was an error processing your request, please try again.");
            }
        }

        private static String FormatRepositoryList(IReadOnlyCollection<String> list)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var repo in list) sb.AppendLine("• " + repo);

            return sb.ToString();
        }

        private static CommandResponse HandleHelp(GitHubUser user, Command command)
        {
            // TODO: move this out into separate file and deserialize in
            Attachment sub = new Attachment
            {
                fallback = "Help Commands",
                pretext = "*Available commands:*",
                title = "/knowhows to [query]",
                text = "Performs a natural language search on a concept",
                color = "#7CD197"
            };

            Attachment sub1 = new Attachment
            {
                fallback = "Help Commands",
                title = "/knowhows search [query]",
                text = "Performs a literal search on a code term",
                color = "#7CD197"
            };

            Attachment sub2 = new Attachment
            {
                fallback = "Help Commands",
                title = "/knowhows track [repository_name | *]",
                text = "Tracks and indexes one or all (*) of your repositories",
                color = "#5397c1"
            };

            Attachment sub3 = new Attachment
            {
                fallback = "Help Commands",
                title = "/knowhows untrack [repository_name | *]",
                text = "Untracks and unindexes one or all (*) of your repositories",
                color = "#5397c1"
            };

            Attachment sub4 = new Attachment
            {
                fallback = "Help Commands",
                title = "/knowhows help",
                text = "Shows this help message",
                color = "#c16883"
            };

            List<Attachment> suby = new List<Attachment>() {
                sub,
                sub1,
                sub2,
                sub3,
                sub4
            };

            return new CommandResponse(suby);
        }
    }
}
