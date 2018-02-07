using System;
using System.Collections.Generic;
using System.Text;
using Website.Manager;
using Website.Managers;
using Website.Utility.Solr;

namespace Website.Commands
{
    public class CommandHandler
    {
        public static CommandResponse HandleCommand(GitHubUser user, Command command)
        {
            try
            {
                string action = command.text.Split(' ')[0];
                switch (action)
                {
                    case "search":  return HandleSearch(user, command);
                    case "track":   return HandleTrack(user, command);
                    case "untrack": return HandleUntrack(user, command);
                    case "help":    return HandleHelp(user, command);
                }
                return HandleHelp(user, command);
            }
            catch (Exception ex) { return new CommandResponse(String.Format("*Error:* {0}", ex.ToString())); }       
        }

        private static CommandResponse HandleSearch(GitHubUser user, Command command)
        {
            string text = command.text, action = text.Split(' ')[0];
            string query = command.text.Substring(text.IndexOf(action) + (action.Length + 1));
            var results = SolrManager.Instance.PerformQuery(query, user.ChannelID);

            StringBuilder sb = new StringBuilder();

            if (results.Count == 0) sb.AppendLine("*No results found*");
            else
            {
                sb.AppendLine(String.Format("Found *{0}* results...", results.Count));

                // should always be a max of 5 results, set in Solr Query
                for (int i = 0; i < results.Count; i++)
                {
                    var result = results[i];
                    // dont return results from questioner
                    if (result.Committer_Name == user.UserID)
                        continue;

                    sb.AppendLine(String.Format("• *<@{0}>* made changes to *{1}* on *{2}* (*{3}*). ",
                        result.Committer_Name,
                        result.Filename,
                        result.Author_Date.ToShortDateString(),
                        result.Html_Url));
                }
            }

            return new CommandResponse(sb.ToString());
        }

        private static CommandResponse HandleTrack(GitHubUser user, Command command)
        {
            string text = command.text, action = text.Split(' ')[0];
            string repository = text.Split(' ').Length >= 2 ? command.text.Substring(text.IndexOf(action) + (action.Length + 1)) : "";

            if (user.TrackRepository(repository)) return new CommandResponse("*Successfully tracked* " + repository);
            else
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("*Here are your available untracked repositories:* ");
                sb.AppendLine(FormatRepositoryList(user.UntrackedRepositories));
                return new CommandResponse(sb.ToString());
            }
        }

        private static CommandResponse HandleUntrack(GitHubUser user, Command command)
        {
            string text = command.text, action = text.Split(' ')[0];
            string repository = text.Split(' ').Length >= 2 ? command.text.Substring(text.IndexOf(action) + (action.Length + 1)) : "";

            if (user.UntrackRepository(repository)) return new CommandResponse("*Successfully untracked* " + repository);
            else
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("*Here are your available tracked repositories:* ");
                sb.AppendLine(FormatRepositoryList(user.TrackedRepositories));
                return new CommandResponse(sb.ToString());
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
            StringBuilder sb = new StringBuilder();

            // @TODO i want to change commands to use c# attributes, so we can associate a function with a command name and description
            // shouldnt be hard coding this stuff here
            sb.AppendLine("*Available commands:* ");

            sb.AppendLine("/knowhows search <query> -- perform a code query search");
            sb.AppendLine("/knowhows track <repository name> -- tracks and indexes one of your repositories");
            sb.AppendLine("/knowhows untrack <repository name> -- untracks and unindexes one of your repositories");
            sb.AppendLine("/knowhows help -- shows this help message");

            return new CommandResponse(sb.ToString());
        }
    }
}
