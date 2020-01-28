using DFC.ServiceTaxonomy.TestSuite.Extensions;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.PageObjects;
using System;
using System.Collections.Generic;
using System.Text;
using TechTalk.SpecFlow;

namespace DFC.ServiceTaxonomy.TestSuite.PageObjects
{
    class PageBase
    {
        private ScenarioContext _scenarioContext;

        public PageBase(ScenarioContext context)
        {
            _scenarioContext = context;
        }
    }
}
