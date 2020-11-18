using System;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.Services;
using Microsoft.AspNetCore.Mvc.Filters;

namespace DFC.ServiceTaxonomy.Editor.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class UnhandledExceptionFilterAttribute : ExceptionFilterAttribute
    {
        private readonly ISlackMessagePublisher _slackMessagePublisher;

        public UnhandledExceptionFilterAttribute(ISlackMessagePublisher slackMessagePublisher)
        {
            _slackMessagePublisher = slackMessagePublisher;
        }

        public override async Task OnExceptionAsync(ExceptionContext context)
        {
            string message = context.Exception.Message;

            if (context.Exception.StackTrace != null)
            {
                message += $"\r\n```{context.Exception.StackTrace}```";
            }

            if (context.Exception.InnerException != null)
            {
                message = message.TrimEnd('`');
                message += $"\r\n\r\nInner Exceptions:";

                var exception = context.Exception;

                while (exception.InnerException != null)
                {
#pragma warning disable S1643 // Strings should not be concatenated using '+' in a loop
                    message += $"\r\n{exception.InnerException.Message}";
#pragma warning restore S1643 // Strings should not be concatenated using '+' in a loop
                    exception = exception.InnerException;
                }

                message += "```";
            }

            await _slackMessagePublisher.SendMessageAsync(message);
        }
    }
}
