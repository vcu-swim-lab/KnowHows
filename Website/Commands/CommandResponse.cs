using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Website.Commands
{
    public class CommandResponse
    {
        public string text { get; set; }
        public CommandResponse(String text) { this.text = text; }
    }
}
