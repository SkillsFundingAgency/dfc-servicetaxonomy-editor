using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
//using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Scripting;

namespace DFC.ServiceTaxonomy.Editor.MethodProviders
{
    //todo: security
    public class ConfigMethodProvider : IGlobalMethodProvider
    {
        private readonly GlobalMethod _globalMethod;

        // can't inject ShellSettings for IShellConfiguration
        // public ConfigMethodProvider(IShellConfiguration configuration)
        public ConfigMethodProvider(IConfiguration configuration)
        {
            _globalMethod = new GlobalMethod
            {
                Name = "config",
                Method = serviceprovider => (Func<string, object>)(name => configuration[name])
            };
        }

        public IEnumerable<GlobalMethod> GetMethods()
        {
            yield return _globalMethod;
        }
    }
}
