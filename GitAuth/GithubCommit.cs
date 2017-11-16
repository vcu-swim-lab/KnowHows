using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitAuth
{
    public class GithubCommit
    {
        public string content { get; set; }

        public string author_name { get; set; }
        public string author_emanil { get; set; }
        public DateTimeOffset author_date { get; set; }

        public string committer_name { get; set; }
        public string committer_email { get; set; }
        public DateTimeOffset committer_date { get; set; }

        public string sha { get; set; }
        public string url { get; set; }

        public string message { get; set; }
        public string raw_url { get; set; }
        public int comment_count { get; set; }

        public string login { get; set; }
        public int id { get; set; }
        public string avatar_url { get; set; }
        public string gravatar_id { get; set; }
        public string author_url { get; set; }
        public string html_url { get; set; }
        public string followers_url { get; set; }
        public string following_url { get; set; }
        public string gists_url { get; set; }
        public string starred_url { get; set; }
        public string subscriptions_url { get; set; }
        public string organizations_url { get; set; }
        public string repos_url { get; set; }
        public string events_url { get; set; }
        public string received_events_url { get; set; }
        public string type { get; set; }
        public bool site_admin { get; set; }

        public string parent_sha { get; set; }
        public string parent_url { get; set; }
        public string parent_html_url { get; set; }

        public int total { get; set; }
        public int additions { get; set; }
        public int deletions { get; set; }


        public string file_sha { get; set; }
        public string filename { get; set; }
        public string file_status { get; set; }
        public int file_additions { get; set; }
        public int file_deletions { get; set; }
        public int changes { get; set; }
        public string blob_url { get; set; }
        public string file_raw_url { get; set; }
        public string contents_url { get; set; }
        public string patch { get; set; }
        public string previous_file_name { get; set; }
    }
}
