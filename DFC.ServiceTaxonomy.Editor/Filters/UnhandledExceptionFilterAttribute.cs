using System;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Slack;
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
            string message = $"```{context.Exception}```";
            await _slackMessagePublisher.SendMessageAsync(message);
        }
    }
}
