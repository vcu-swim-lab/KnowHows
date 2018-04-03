using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Website.Commands
{
    public class CommandResponse
    {
        public string text { get; set; }
        public List<Attachment> attachments { get; set; }
        public CommandResponse(String text) { this.text = text; }
        public CommandResponse(List<Attachment> attachment) { this.attachments = attachment; }
    }

    public class Attachment
    {
        public string fallback { get; set; }
        public string color { get; set; }
        public string pretext { get; set; }
        public string author_name { get; set; }
        public string author_link { get; set; }
        public string author_icon { get; set; }
        public string title { get; set; }
        public string title_link { get; set; }
        public string text { get; set; }
        public List<Field> fields { get; set; }
        public string image_url { get; set; }
        public string thumb_url { get; set; }
        public string footer { get; set; }
        public string footer_icon { get; set; }
        public int ts { get; set; }
    }

    public class Field
    {
        public string title { get; set; }
        public string value { get; set; }
        public bool @short { get; set; }
    }
}
