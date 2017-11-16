using SolrNet.Attributes;
using System;
using System.IO;

namespace SolrServices
{
    public class CodeDoc
    {
        [SolrUniqueKey("id")]
        public string Id { get; set; }

        [SolrField("sha")]
        public string Sha { get; set; }

        [SolrField("author_name")]
        public string Author_Name { get; set; }

        [SolrField("committer_name")]
        public string Committer_Name { get; set; }

        [SolrField("author_date")]
        public DateTime Author_Date { get; set; }

        [SolrField("filename")]
        public string Filename { get; set; }

        [SolrField("previous_file_name")]
        public string Previous_File_Name { get; set; }

        [SolrField("blob_url")]
        public string Blob_Url { get; set; }

        [SolrField("raw_url")]
        public string Raw_Url { get; set; }

        [SolrField("content")]
        public string Content { get; set; }

        [SolrField("accesstoken")]
        public string Accesstoken { get; set; }
    }
}