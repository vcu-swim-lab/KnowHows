using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Website.Utility;

namespace Website
{
    public class Program
    {
        private const int WEBSITE_PORT = 53222;

        public static IConfigurationRoot Configuration { get; set; }

        public static void Main(string[] args)
        {
            Configuration = new ConfigurationBuilder()
                        .AddCommandLine(args)
                        .AddJsonFile("appsettings.json", optional: false)
                        .Build();

            var host = new WebHostBuilder()
                        .UseKestrel()
                        .UseConfiguration(Configuration)
                        .UseContentRoot(Directory.GetCurrentDirectory())
                        .UseIISIntegration()
                        .UseStartup<Startup>()
                        .UseUrls("http://*:" + WEBSITE_PORT)
                        .Build();

            host.Run();
        }
    }
}
