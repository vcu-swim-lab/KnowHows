using System;
using System.IO;
using System.Reflection;

using Noobot.Core.Configuration;
using Topshelf;
using System.Collections.Generic;
using SolrServices;
using GitAuth;

namespace Swim.HelpMeCode.ConsoleService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
            Console.WriteLine($"Noobot.Core assembly version: {Assembly.GetAssembly(typeof(Noobot.Core.INoobotCore)).GetName().Version}");

            HostFactory.Run(x =>
            {
                x.Service<NoobotHost>(s =>
                {
                    s.ConstructUsing(name => new NoobotHost(new JsonConfigReader()));
                    s.WhenStarted(n => n.Start());
                    s.WhenStopped(n => n.Stop());
                });

                x.RunAsNetworkService();
                x.SetDisplayName("HelpMeCode");
                x.SetServiceName("HelpMeCode");
                x.SetDescription("A bot to help you code");
            });
        }

        public static List<CodeDoc> MapHubToSolr(List<GithubCommit> coms, string token)
        {
            List<CodeDoc> solrStuff = new List<CodeDoc>();
            int num = 0;

            foreach (var commit in coms)
            {
                CodeDoc code = new CodeDoc();
                code.Author_Date = commit.author_date.DateTime;
                code.Author_Name = commit.author_name;
                code.Blob_Url = commit.blob_url;
                code.Committer_Name = commit.committer_name;
                code.Content = commit.content;
                code.Filename = commit.filename;
                code.Accesstoken = token;
                code.Id = "somechannelthingyiguess" + num; // note solrconfig should be able to generate a unique id, should be...
                code.Previous_File_Name = commit.previous_file_name;
                code.Raw_Url = commit.raw_url;
                code.Sha = commit.sha;
                num++;
                solrStuff.Add(code);
            }

            return solrStuff;
        }
    }
}
