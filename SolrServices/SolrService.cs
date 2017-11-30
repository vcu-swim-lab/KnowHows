using Microsoft.Practices.ServiceLocation;
using SolrNet;
using SolrNet.Commands.Parameters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace SolrServices
{
    public class SolrService
    {
        string connection = "http://localhost:8983/solr/newish_core";

        public SolrService()
        {
            Startup.Init<CodeDoc>(connection);
        }

        /// <summary>
        /// this is how we should be entering our data in, for searchability
        /// </summary>
        /// <param name="incoming"></param>
        public async Task AddIndexed(List<CodeDoc> incoming)
        {
            ISolrOperations<CodeDoc> solr = ServiceLocator.Current.GetInstance<ISolrOperations<CodeDoc>>();

            // this works better, can log bad responses if need be. response 1 == bad
            foreach (var inc in incoming)
            {
                var send = await solr.AddAsync(inc);
                write(send.Status.ToString());
            }

            solr.Commit();
        }

        /// <summary>
        /// Provide a search string and filter string this will return the top result.  
        /// </summary>
        /// <param name="search">the search term</param>
        /// <param name="channelId">the channel to filter by</param>
        public void Query(string search, string channelId)
        {
            ISolrOperations<CodeDoc> solr = ServiceLocator.Current.GetInstance<ISolrOperations<CodeDoc>>();
            List<ISolrQuery> filter = new List<ISolrQuery>();
            filter.Add(new SolrQueryByField("channel", channelId));
            var opts = new QueryOptions();
            opts.ExtraParams = new KeyValuePair<string, string>[]
            {
                new KeyValuePair<string, string>("wt", "xml") // wt = writertype (response format)
            };

            // this should add an additional filter by channel ID 
            // this removes cross contamination
            foreach (var filt in filter)
            {
                opts.AddFilterQueries(filt);
            }

            var query = new SolrQuery(search);
            var codeQuery = solr.Query(query, opts);

            // pulling the top-ranked author
            if (codeQuery.Count > 0)
                write("Author: " + codeQuery[0].Author_Name);
            else
                write("nothing returned, try again");
            foreach (CodeDoc codeDoc in codeQuery)
            {
                //write("Found " + codeDoc.Name);
            }

        }

        /// <summary>
        /// getting weird errors but it sends the documents
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public async Task AddWithoutIndex(string filePath, string fileName)
        {
            ISolrOperations<CodeDoc> solr = ServiceLocator.Current.GetInstance<ISolrOperations<CodeDoc>>();

            using (var file = File.OpenRead(filePath))
            {
                var resp = await solr.ExtractAsync(new ExtractParameters(file, "main.go")
                {
                    ExtractOnly = false,
                    ExtractFormat = ExtractFormat.Text
                });
                solr.Commit();
                write(resp.Content);
            }
        }

        /// <summary>
        /// Need to work on this.
        /// </summary>
        public void GenericQuery()
        {
            ISolrOperations<CodeDoc> solr = ServiceLocator.Current.GetInstance<ISolrOperations<CodeDoc>>();

            var codeQuery = solr.Query(new SolrQuery("main.go"));

            //var q = new SolrHasValueQuery("name");

            foreach (CodeDoc codeDoc in codeQuery)
            {
                //write("Found " + codeDoc.Name);
            }

        }

        public void write(string write)
        {
            Console.WriteLine(write);
            //Logging.Log.Content.Trace(write);
        }
    }
}
