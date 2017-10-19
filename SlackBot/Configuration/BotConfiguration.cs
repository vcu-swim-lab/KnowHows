using Noobot.Core.Configuration;
using Swim.HelpMeCode.ConsoleService.MiddleWare;

namespace Swim.HelpMeCode.ConsoleService.Configuration
{
    public class BotConfiguration : ConfigurationBase
    {
        public BotConfiguration()
        {
            UseMiddleware<CodeSearchMiddleWare>();
        }
    }
}