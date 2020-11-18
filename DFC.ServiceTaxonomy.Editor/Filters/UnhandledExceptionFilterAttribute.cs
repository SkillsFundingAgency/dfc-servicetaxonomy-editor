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
            await _slackMessagePublisher.SendMessageAsync(context.Exception.Message);

            if (!string.IsNullOrWhiteSpace(context.Exception.StackTrace))
            {
                await _slackMessagePublisher.SendMessageAsync($"```{context.Exception.StackTrace}```");
            }
        }
    }
}
