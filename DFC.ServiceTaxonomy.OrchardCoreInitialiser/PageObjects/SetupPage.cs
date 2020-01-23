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
            try
            {
                _webDriver.FindElement(By.Id("SiteName")).SendKeys(siteName);
            }
            catch ( Exception e)
            {
                throw new Exception("Setup page: Unable to enter a value for SiteName", e);
            }
            return this;
        }

        public SetupPage selectRecipe(string recipeName )
        {
            try
            {
                _webDriver.FindElement(By.Id("recipeButton")).Click();
                _webDriver.FindElement(By.XPath("//span[contains(.,'" + recipeName +"')]")).Click();
            }
            catch( Exception e)
            {
                throw new Exception("Setup page: Unable to select a Recipe", e);
            }
            return this;
        }

        public SetupPage selectDefaultTimeZone()
        {
            try
            {
                var timeZone = _webDriver.FindElement(By.Id("SiteTimeZone"));
                var selectElement = new SelectElement(timeZone);
                selectElement.SelectByValue("label=(GMT+00) Europe/London (+00:00)");
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception in function selectDefaultTimeZone: " + e);
            }

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
                Console.WriteLine("Exception in function selectDatabase: " + e);
            }

            return this;
        }

        public SetupPage enterTablePrefix(string tablePrefix)
        {
            try
            {
                _webDriver.FindElement(By.Id("TablePrefix")).SendKeys(tablePrefix);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception in function enterTablePrefix: " + e);
            }

            return this;
        }

        public SetupPage enterConnectionString(string connectionString)
        {
            try
            {
                _webDriver.FindElement(By.Id("ConnectionString")).SendKeys(connectionString);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception in function enterConnectionString: " + e);
            }

            return this;
        }


        public SetupPage enterUsername(string username)
        {
            try
            {
                _webDriver.FindElement(By.Id("UserName")).SendKeys(username);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception in function enterUsername: " + e);
            }
 
            return this;
        }

        public SetupPage enterEmail(string emailAddress)
        {
            try
            {
                _webDriver.FindElement(By.Id("Email")).SendKeys(emailAddress);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception in function enterEmail: " + e);
            }
 
            return this;
        }

        public SetupPage enterPassword(string password)
        {
            try
            {
                _webDriver.FindElement(By.Id("Password")).SendKeys(password);
                _webDriver.FindElement(By.Id("PasswordConfirmation")).SendKeys(password);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception in function enterPassword: " + e);
            }
 
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
                //TODO: this should throw
                Console.WriteLine("Exception in function submitForm: " + e);
            }
            
            return this;
        }
    }
}


