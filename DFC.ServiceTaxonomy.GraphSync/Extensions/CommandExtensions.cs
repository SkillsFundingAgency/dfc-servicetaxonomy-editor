using System;
using System.Linq;
using DFC.ServiceTaxonomy.GraphSync.Interfaces;

namespace DFC.ServiceTaxonomy.GraphSync.Extensions
{
    public static class CommandExtensions
    {
        public static void CheckIsValid(this ICommand command)
        {
            try
            {
                var validationErrors = command?.ValidationErrors();

                if (validationErrors?.Any() == true)
                {
                    throw new InvalidOperationException(@$"{command?.GetType().Name} not valid:
{string.Join(Environment.NewLine, validationErrors)}");
                }
            }
            catch (Exception ex)
            {
#pragma warning disable CA2200 // Rethrow to preserve stack details
#pragma warning disable S3445 // Exceptions should not be explicitly rethrown
                throw ex;
#pragma warning restore S3445 // Exceptions should not be explicitly rethrown
#pragma warning restore CA2200 // Rethrow to preserve stack details
            }
        }
    }
}
