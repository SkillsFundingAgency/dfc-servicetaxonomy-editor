using OpenQA.Selenium;
using OpenQA.Selenium.Support.PageObjects;
using System;
using System.Collections.Generic;
using System.Text;


namespace DFC.ServiceTaxonomy.OrchardCoreInitialiser.PageObjects
{
    class LogonScreen
    {

        IWebDriver _webDriver;
        public LogonScreen(IWebDriver driver)
        {
            _webDriver = driver;
        }

        public LogonScreen navigateToLoginPage(string url)
        {
            _webDriver.Url = url;
            return this;
        }

        public LogonScreen enterUsername(string username)
        {
            try
            {
                _webDriver.FindElement(By.Id("UserName")).SendKeys(username);
            }
            catch( Exception e)
            {
                throw new Exception("Error in function EnterUsername:\n", e);
            }
            return this;
        }

        public LogonScreen enterPassword(string password)
        {
            try
            {
                _webDriver.FindElement(By.Id("Password")).SendKeys(password);
                _webDriver.FindElement(By.Id("Password")).SendKeys(Keys.Return);
            }
            catch (Exception e)
            {
                throw new Exception("Error in function enterPassword:\n", e);
            }
            
            return this;
        }

        public StartPage SubmitLogonDetails ( string editorBaseUrl, string editorUid, string editorPassword)
        {
            navigateToLoginPage(editorBaseUrl);
            enterUsername(editorUid);
            enterPassword(editorPassword);
            // check no validation errors
            var elements = _webDriver.FindElements(By.ClassName("text-danger"));
            if ( elements.Count > 0)
            {
                throw new Exception("Error in function SubmitLogonDetails: " + elements[0].Text);
            }
            return new StartPage(_webDriver);
        }
    }


}
