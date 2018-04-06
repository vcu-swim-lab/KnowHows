
namespace Website.Utility
{
    public class AppSettings
    {
        // GitHub Settings
        public string GITHUB_APP_CLIENT_ID { get; set; }
        public string GITHUB_APP_CLIENT_SECRET { get; set; }
        public string GITHUB_APP_OAUTH_REDIRECT_URL { get; set; }

        // Slack Settings
        public string SLACK_APP_CLIENT_ID { get; set; }
        public string SLACK_APP_CLIENT_SECRET { get; set; }
        public string SLACK_APP_OAUTH_REDIRECT_URL { get; set; }

        // Solr Settings
        public string SOLR_URL { get; set; }

        // Website Settings
        public string WEBSITE_BASE_URL { get; set; }
        public string WEBSITE_PORT { get; set; }

    }
}
