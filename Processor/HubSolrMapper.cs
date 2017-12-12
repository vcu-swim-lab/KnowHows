using SolrServices;
using System.Collections.Generic;

namespace Processor
{
    public class HubSolrMapper
    {
        /// <summary>
        /// This will take a list of GitHub commits and convert them into a List 
        /// ready to be put into Solr. 
        /// </summary>
        /// <param name="coms">Commits</param>
        /// <param name="toke">access token</param>
        /// <returns></returns>
        /*public List<CodeDoc> HubToSolr(List<GithubCommit> coms, string toke)
        {
            // TODO: get channel Id from slack to filter with later. 
            List<CodeDoc> solrStuff = new List<CodeDoc>();
            int num = 30;
            int blah = 1;
            foreach (var commit in coms)
            {
                CodeDoc code = new CodeDoc();
                code.Author_Date = commit.author_date.DateTime;
                code.Author_Name = commit.author_name;
                code.Blob_Url = commit.blob_url;
                code.Committer_Name = commit.committer_name;
                code.Patch = commit.patch;
                code.Content = commit.content;
                code.Filename = commit.filename;
                code.Id = "somechannelthingyiguess" + num;
                code.Accesstoken = toke;
                code.Previous_File_Name = commit.previous_file_name;
                code.Raw_Url = commit.raw_url;
                code.Sha = commit.sha;
                code.Channel = "channel" + blah;
                if (num % 10 == 0)
                {
                    blah += 2;
                }
                num++;
                solrStuff.Add(code);
            }

            return solrStuff;
        }*/
    }
}
