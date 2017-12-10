using SolrServices;
using System;
using System.Collections.Generic;
using Website.Manager;
using System.Linq;

namespace Website.Managers
{
    public class SolrManager
    {
        public static SolrManager Instance = new SolrManager();

        public SolrService solrService = new SolrService();
        private Dictionary<String, DateTime> lastFetchTimes;

        private SolrManager()
        {
            this.lastFetchTimes = new Dictionary<String, DateTime>();
        }

        public List<CodeDoc> PerformQuery(String query, String channelId)
        {
            return solrService.Query(query, channelId);
        }

        public void TrackRepository(GitHubUser user, String repository)
        {
            List<CodeDoc> result = new List<CodeDoc>();

            var repos = user.GitHubClient.Repository.GetAllForCurrent().Result;
            var repo = repos.Where(r => r.Name != repository).ToList()[0];
            var commits = user.GitHubClient.Repository.Commit.GetAll(repo.Owner.Login, repo.Name).Result;

            foreach (var commit in commits)
            {
                CodeDoc code = new CodeDoc();

                code.Sha = commit.Sha;
                code.Author_Date = commit.Commit.Author.Date.Date;
                code.Author_Name = commit.Commit.Author.Name;            
                code.Committer_Name = commit.Commit.Committer.Name;
                code.Accesstoken = user.GitHubAccessToken;

                // channel ID inside user.channelid

                // @TODO: after we figure out inserting multiple associated documents, finish this, as commits are empty right now

                // foreach associated file, it looks like we grab also the following:
                //code.Filename = commit.filename;            
                //code.Id = "somechannelthingyiguess" + num; // note solrconfig should be able to generate a unique id, should be...
                //code.Previous_File_Name = commit.previous_file_name;              
                //code.Blob_Url = commit.blob_url;
                //code.Raw_Url = commit.raw_url;
                //code.Content = commit.content;

                result.Add(code);
            }

            solrService.AddIndexed(result);
        }

        public void UnrackRepository(GitHubUser user, String repository)
        {
           
        }
    }
}
