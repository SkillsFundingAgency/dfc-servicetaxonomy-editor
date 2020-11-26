using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace DFC.ServiceTaxonomy.Slack.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class UnhandledExceptionFilterAttribute : ExceptionFilterAttribute
    {
        private readonly ISlackMessagePublisher _slackMessagePublisher;
        private readonly ILogger _logger;

        public UnhandledExceptionFilterAttribute(ISlackMessagePublisher slackMessagePublisher, ILogger<UnhandledExceptionFilterAttribute> logger)
        {
            _slackMessagePublisher = slackMessagePublisher;
            _logger = logger;
        }

        public override async Task OnExceptionAsync(ExceptionContext context)
        {
            try
            {
                string message = $"```{context.Exception}```";
                await _slackMessagePublisher.SendMessageAsync(message);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
            }
        }
    }
}
