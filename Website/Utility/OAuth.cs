using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Website.Utility
{
    public static class OAuth
    {
        public class OAuthResponse
        {
            public string access_token { get; set; }
            public string scope { get; set; }
        }

        public class OAuthRequest
        {
            public string code { get; set; }
            public string state { get; set; }
        }
    }
}
