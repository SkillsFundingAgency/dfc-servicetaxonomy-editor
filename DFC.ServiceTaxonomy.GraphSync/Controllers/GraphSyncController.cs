using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.Services.Interface;
using Microsoft.AspNetCore.Mvc;

namespace DFC.ServiceTaxonomy.GraphSync.Controllers
{
    public class GraphSyncController : Controller
    {
        private readonly ISynonymService _synonymService;

        public GraphSyncController(ISynonymService synonymService)
        {
            _synonymService = synonymService;
        }

        [HttpGet("GraphSync/Synonyms/{node}/{filename}")]
        public async Task<IActionResult> Synonyms(string node, string filename)
        {
            var synonyms = await _synonymService.GetSynonymsAsync(node);

            var sb = new StringBuilder();
            foreach (var item in synonyms)
            {
                sb.AppendLine(item);
            }

            var stream = new MemoryStream(buffer: Encoding.UTF8.GetBytes(sb.ToString()));

            try
            {
                stream.Position = 0;
                return new FileStreamResult(stream, "text/plain")
                {
                    FileDownloadName = filename
                };
            }
            catch
            {
                stream.Dispose();
                throw;
            }
        }

        public ActionResult Index()
        {
            return View();
        }
    }
}
