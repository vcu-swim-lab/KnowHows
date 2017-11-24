using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Website.Utility.OAuth
{
    public class OAuthRequest
    {
        public string code { get; set; }
        public string state { get; set; }
    }
}
