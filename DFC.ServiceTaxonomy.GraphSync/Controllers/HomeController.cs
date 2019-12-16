using Microsoft.AspNetCore.Mvc;

namespace DFC.ServiceTaxonomy.GraphSync.Controllers
{
    //todo: should this module also contain the graph sync activity? probably
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}
