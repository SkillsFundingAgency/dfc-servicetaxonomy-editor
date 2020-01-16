using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium.Support.PageObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace DFC.ServiceTaxonomy.OrchardCoreInitialiser.PageObjects
{
    class SetupPage
    {
        private IWebDriver _webDriver;
        //EnvironmentSettings env = new EnvironmentSettings();
        //private ScenarioContext _scenarioContext;

        public SetupPage(IWebDriver driver)
        {
            _webDriver = driver;

        }

        public SetupPage openPage(string url)
        {
            _webDriver.Navigate().GoToUrl(url);
            return this;
        }

        public SetupPage enterSiteName(string siteName)
        {
            _webDriver.FindElement(By.Id("SiteName")).SendKeys(siteName);
            return this;
        }

        public SetupPage selectRecipe(string recipeName)
        {
            try
            {
                //xpath =//span[contains(.,'Sets up the Service Taxonomy Editor')]
                _webDriver.FindElement(By.Id("recipeButton")).Click();
                _webDriver.FindElement(By.XPath("//span[contains(.,'Sets up the Service Taxonomy Editor')]")).Click();
                //_webDriver.FindElement(By.CssSelector("[recipe-display-name: \"Service Taxonomy Editor\"]")).Click();
            }
            catch( Exception e)
            {

            }
            
            //var recipeList = _webDriver.FindElement(By.Id("recipeButton"));
            //var selectElement = new SelectElement(recipeList);
            //selectElement.SelectByText(recipeName);
            return this;
        }

        public SetupPage selectDefaultTimeZone()
        {
            var timeZone = _webDriver.FindElement(By.Id("SiteTimeZone"));
            var selectElement = new SelectElement(timeZone);
            selectElement.SelectByValue("label=(GMT+00) Europe/London (+00:00)");
            return this;
        }

        public SetupPage selectDatabase( string databaseType)
        {
            try
            {
                var databaseList = _webDriver.FindElement(By.Id("DatabaseProvider"));
                var selectElement = new SelectElement(databaseList);
                selectElement.SelectByValue(databaseType);
            }
            catch (Exception e)
            {

            }

            return this;
        }

        public SetupPage enterUsername(string username)
        {
            _webDriver.FindElement(By.Id("UserName")).SendKeys(username);
            return this;
        }

        public SetupPage enterEmail(string emailAddress)
        {
            _webDriver.FindElement(By.Id("Email")).SendKeys(emailAddress);
            return this;
        }

        public SetupPage enterPassword(string password)
        {
            _webDriver.FindElement(By.Id("Password")).SendKeys(password);
            _webDriver.FindElement(By.Id("PasswordConfirmation")).SendKeys(password);
            return this;
        }

        public SetupPage submitForm()
        {
            try
            {
                _webDriver.FindElement(By.Id("SubmitButton")).Click();
            }
            catch (Exception e)
            {

            }
            
            return this;
        }
    }
}


