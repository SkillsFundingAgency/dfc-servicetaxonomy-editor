using System;
using System.IO;
using System.Text;
using DFC.ServiceTaxonomy.GraphSync.Services.Interface;
using Microsoft.AspNetCore.Mvc;

namespace DFC.ServiceTaxonomy.GraphSync.Controllers
{
    public class HomeController : Controller
    {
        private readonly ISynonymService _synonymService;

        public HomeController(ISynonymService synonymService)
        {
            _synonymService = synonymService ?? throw new ArgumentNullException(nameof(synonymService));
        }

        [HttpGet("GraphSync/Synonyms/{filename}")]
        public IActionResult Synonyms(string filename)
        {
            var synonyms = _synonymService.GetSynonyms();

            var sb = new StringBuilder();
            foreach(var item in synonyms)
            {
                sb.AppendLine(item);
            }

            var stream = new MemoryStream(buffer: Encoding.UTF8.GetBytes(sb.ToString()));

            stream.Position = 0;
            return new FileStreamResult(stream, "text/plain")
            {
                FileDownloadName = filename
            };
        }

        public static Stream GenerateStreamFromString(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        public ActionResult Index()
        {
            return View();
        }
    }
}
