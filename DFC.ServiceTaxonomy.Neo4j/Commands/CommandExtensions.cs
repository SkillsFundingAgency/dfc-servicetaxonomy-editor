using System;
using System.Collections.Generic;
using System.Linq;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;

namespace DFC.ServiceTaxonomy.Neo4j.Commands
{
    public static class CommandExtensions
    {
        public static void CheckIsValid(this ICommand command)
        {
            List<string> validationErrors = command.ValidationErrors();
            if (validationErrors.Any())
                throw new InvalidOperationException(@$"{command.GetType().Name} not valid:
{string.Join(Environment.NewLine, validationErrors)}");
        }
    }
}
