﻿using Microsoft.AspNetCore.Mvc;

namespace DFC.ServiceTaxonomy.GraphSync.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index([FromBody]string test)
        {
            return View();
        }
    }
}
