using DFC.ServiceTaxonomy.TestSuite.Extensions;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.PageObjects;
using System;
using System.Collections.Generic;
using System.Text;
using TechTalk.SpecFlow;


namespace DFC.ServiceTaxonomy.TestSuite.PageObjects
{
    class LogonScreen
    {
        private ScenarioContext _scenarioContext;

        public LogonScreen(ScenarioContext context)
        {
            _scenarioContext = context;
        }

        public LogonScreen navigateToLoginPage(string url)
        {
            _scenarioContext.GetWebDriver().Url = url;
            return this;
        }

        public LogonScreen enterUsername(string username)
        {
            _scenarioContext.GetWebDriver().FindElement(By.Id("UserName")).SendKeys(username);
            return this;
        }

        public LogonScreen enterPassword(string password)
        {
            _scenarioContext.GetWebDriver().FindElement(By.Id("Password")).SendKeys(password);
            _scenarioContext.GetWebDriver().FindElement(By.Id("Password")).SendKeys(Keys.Return);
            return this;
        }

        public StartPage SubmitLogonDetails ()
        {
            navigateToLoginPage(_scenarioContext.GetEnv().editorBaseUrl);
            enterUsername(_scenarioContext.GetEnv().editorUid);
            enterPassword(_scenarioContext.GetEnv().editorPassword);
            return new StartPage(_scenarioContext);
        }
    }


}
