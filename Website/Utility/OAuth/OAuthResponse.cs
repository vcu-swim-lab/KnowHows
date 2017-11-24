using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Website.Utility.OAuth
{
    public class OAuthResponse
    {
        public string access_token { get; set; }
        public string scope { get; set; }
    }
}
