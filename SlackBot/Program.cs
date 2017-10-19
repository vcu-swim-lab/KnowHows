using System;
using System.IO;
using System.Reflection;

using Noobot.Core.Configuration;
using Topshelf;

namespace Swim.HelpMeCode.ConsoleService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
            Console.WriteLine($"Noobot.Core assembly version: {Assembly.GetAssembly(typeof(Noobot.Core.INoobotCore)).GetName().Version}");

            HostFactory.Run(x =>
            {
                x.Service<NoobotHost>(s =>
                {
                    s.ConstructUsing(name => new NoobotHost(new JsonConfigReader()));
                    s.WhenStarted(n => n.Start());
                    s.WhenStopped(n => n.Stop());
                });

                x.RunAsNetworkService();
                x.SetDisplayName("HelpMeCode");
                x.SetServiceName("HelpMeCode");
                x.SetDescription("A bot to help you code");
            });
        }
    }
}
