using System.IO;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace Website
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var config = new ConfigurationBuilder()
                        .AddCommandLine(args)
                        .AddJsonFile("appsettings.json", optional: false)
                        .Build();

            var host = new WebHostBuilder()
                        .UseKestrel()
                        .UseConfiguration(config)
                        .UseContentRoot(Directory.GetCurrentDirectory())
                        .UseIISIntegration()
                        .UseStartup<Startup>()
                        .Build();

            host.Run();
        }
    }
}
