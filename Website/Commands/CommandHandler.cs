using System;
using System.Text;
using Website.Manager;
using Website.Managers;

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
            catch (Exception ex) { return new CommandResponse(String.Format("Error: {0}", ex.ToString())); }       
        }

        private static CommandResponse HandleSearch(GitHubUser user, Command command)
        {
            string text = command.text, action = text.Split(' ')[0];
            string query = command.text.Substring(text.IndexOf(action) + (action.Length + 1));
            var results = SolrManager.Instance.PerformQuery(query, user.ChannelID);

            StringBuilder sb = new StringBuilder();
            if (results.Count == 0) sb.AppendLine("No results found");
            else foreach (var result in results) sb.AppendLine(String.Format("{0} <@{1}>", result.Filename, result.Committer_Name));
            return new CommandResponse(sb.ToString());
        }

        private static CommandResponse HandleTrack(GitHubUser user, Command command)
        {       
            string text = command.text, action = text.Split(' ')[0];
            string repository = command.text.Substring(text.IndexOf(action) + (action.Length + 1));

            if (user.TrackRepository(repository)) return new CommandResponse("Successfully tracked " + repository);
            else
            {
                StringBuilder sb = new StringBuilder();

                sb.AppendLine("The repository specified was not recognized, here are your available untracked repositories: ");
                foreach (var repo in user.UntrackedRepositories) sb.AppendLine(repo);

                return new CommandResponse(sb.ToString());
            }
        }

        private static CommandResponse HandleUntrack(GitHubUser user, Command command)
        {      
            string text = command.text, action = text.Split(' ')[0];
            string repository = command.text.Substring(text.IndexOf(action) + (action.Length + 1));

            if (user.UntrackRepository(repository)) return new CommandResponse("Successfully untracked " + repository);
            else
            {
                StringBuilder sb = new StringBuilder();

                sb.AppendLine("The repository specified was not recognized, here are your tracked repositories: ");
                foreach (var repo in user.TrackedRepositories) sb.AppendLine(repo);

                return new CommandResponse(sb.ToString());
            }
        }

        private static CommandResponse HandleHelp(GitHubUser user, Command command)
        {
            StringBuilder sb = new StringBuilder();

            // @TODO i want to change commands to use c# attributes, so we can associate a function with a command name and description
            // shouldnt be hard coding this stuff here
            sb.AppendLine("Available commands: ");

            sb.AppendLine("/knowhows search <query> -- perform a code query search");
            sb.AppendLine("/knowhows track <repository name> -- tracks and indexes one of your repositories");
            sb.AppendLine("/knowhows untrack <repository name> -- untracks and unindexes one of your repositories");
            sb.AppendLine("/knowhows help -- shows this help message");

            return new CommandResponse(sb.ToString());
        }
    }
}
