using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GitAuth;

namespace SlackBot
{
    class Program
    {
        static void Main(string[] args)
        {
            //SlackClient test = new SlackClient("clientId", "clientSecret", "teamName");

            GitAuthenticator aut = new GitAuthenticator();
            aut.oAuth();
            Console.ReadKey();
        }
    }
}
