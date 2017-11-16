using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Website
{
    public class Program
    {
        private const int WEBSITE_PORT = 53222;
        public static string WEBSITE_BASE_URL = "http://localhost:" + WEBSITE_PORT + "/";

        public static void Main(string[] args)
        {
            var config = new ConfigurationBuilder().AddCommandLine(args).Build();
            var host = new WebHostBuilder()
                        .UseKestrel()
                        .UseConfiguration(config)
                        .UseContentRoot(Directory.GetCurrentDirectory())
                        .UseIISIntegration()
                        .UseStartup<Startup>()
                        .UseUrls("http://0.0.0.0:" + WEBSITE_PORT)
                        .Build();
            host.Run();
        }
    }
}
