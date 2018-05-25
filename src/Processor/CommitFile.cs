using System;

namespace Processor
{
    public class CommitFile
    {
        public string Commit_Sha { get; set; }
        public string Filename { get; set; }
        public string Previous_Filename { get; set; }
        public string Author_Name { get; set; }
        public string Authored_Date { get; set; }
        public string Repository { get; set; }
        public string Raw_Url { get; set; }
        public string Blob_Url { get; set; }
        public string Commit_Url { get; set; }
        public string Commit_Message { get; set; }
        public string Parsed_Patch { get; set; }

    }
}