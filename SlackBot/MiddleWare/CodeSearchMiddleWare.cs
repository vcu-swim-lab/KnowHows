using Noobot.Core.MessagingPipeline.Middleware;
using Noobot.Core.MessagingPipeline.Middleware.ValidHandles;
using Noobot.Core.MessagingPipeline.Request;
using Noobot.Core.MessagingPipeline.Response;

using System;
using System.Collections.Generic;

namespace Swim.HelpMeCode.ConsoleService.MiddleWare
{
    public class CodeSearchMiddleWare : MiddlewareBase
    {
        public CodeSearchMiddleWare(IMiddleware next) : base(next)
        {
            HandlerMappings = new[]
            {
                new HandlerMapping
                {
                    ValidHandles = ContainsTextHandle.For("codesearch", "search"),
                    Description = "Searches for code repositories for information regarding the search string specified. Usage: searchcode <search query>",
                    EvaluatorFunc = SearchHandler
                }
            };
        }

        private IEnumerable<ResponseMessage> SearchHandler(IncomingMessage message, IValidHandle matchedHandle)
        {
            yield return message.IndicateTypingOnChannel();

            var text = message.FullText;
            String searchQuery = text.Substring(text.IndexOf(" ") + 1);

            // @TODO process their search query
            String result = String.Format("*Received search request:* {0}", searchQuery);

            yield return message.ReplyToChannel(result);
        }
    }
}
