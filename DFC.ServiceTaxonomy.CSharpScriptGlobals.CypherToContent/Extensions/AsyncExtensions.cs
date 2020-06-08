using System;
using System.Threading.Tasks;

namespace DFC.ServiceTaxonomy.CSharpScriptGlobals.CypherToContent.Extensions
{
    public static class AsyncExtensions
    {
#pragma warning disable S3168 // "async" methods should not return "void"
        public static async void FireAndForget(this Task task)
#pragma warning restore S3168 // "async" methods should not return "void"
        {
            try
            {
                await task;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error in FireAndForget:{e.Message}");
                // log errors
            }
        }
    }
}
