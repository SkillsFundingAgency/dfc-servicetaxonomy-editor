using DFC.ServiceTaxonomy.TestSuite.Extensions;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.PageObjects;
using System;
using System.Collections.Generic;
using System.Text;
using TechTalk.SpecFlow;


namespace DFC.ServiceTaxonomy.TestSuite.PageObjects
{
    class StartPage
    {
        //private IWebDriver driver;
        //EnvironmentSettings env = new EnvironmentSettings();
        private ScenarioContext _scenarioContext;

        public StartPage(ScenarioContext context)
        {
            _scenarioContext = context;

        }

        public StartPage NavigateTo( string sPath)
        {
            _scenarioContext.GetWebDriver().Url = _scenarioContext.GetEnv().editorBaseUrl + "/" + sPath;
            return this;
        }


        public AddActivity NavigateToNewActivity()
        {
            _scenarioContext.GetWebDriver().Url = _scenarioContext.GetEnv().editorBaseUrl + "/Admin/Contents/ContentTypes/Activity/Create";
            return new AddActivity (_scenarioContext);
        }

    }
}
