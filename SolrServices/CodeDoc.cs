using SolrNet.Attributes;
using System.IO;

namespace SolrServices
{
    public class CodeDoc
    {
        [SolrUniqueKey("name")]
        public string Name { get; set; }

        [SolrField("internals")]
        public string Internals { get; set; }

        [SolrField("author")]
        public string Author { get; set; }

        [SolrField("commitId")]
        public string CommitId { get; set; }
    }
}