using GitAuth;
using Microsoft.Practices.ServiceLocation;
using SolrNet;
using SolrServices;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
        /// <param name="filePath"></param>
        /// <param name="fileName"></param>
        public void AddIndexed(List<CodeDoc> incoming)
        {
            ISolrOperations<CodeDoc> solr = ServiceLocator.Current.GetInstance<ISolrOperations<CodeDoc>>();

            incoming.ForEach(async x => { await solr.AddAsync(x); solr.Commit(); });

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
