using System;
using Microsoft.Extensions.Logging;
using ILogger = Neo4j.Driver.ILogger;

namespace DFC.ServiceTaxonomy.Neo4j.Log
{
    public class NeoLogger : ILogger
    {
        private readonly ILogger<NeoLogger> _logger;

        public NeoLogger(ILogger<NeoLogger> logger)
        {
            _logger = logger;
        }

        public void Error(Exception cause, string message, params object[] args) => _logger.LogError(cause, message, args);

        public void Warn(Exception cause, string message, params object[] args) => _logger.LogWarning(cause, message, args);

        public void Info(string message, params object[] args) => _logger.LogInformation(message, args);

        //todo: have setting to selectively log debug lines (e.g. starts with "RUN")
        public void Debug(string message, params object[] args) => _logger.LogDebug(message, args);

        public void Trace(string message, params object[] args) => _logger.LogTrace(message, args);

        public bool IsTraceEnabled() => _logger.IsEnabled(LogLevel.Trace);

        public bool IsDebugEnabled() => _logger.IsEnabled(LogLevel.Debug);
    }
}
