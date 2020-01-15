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
            _webDriver.FindElement(By.Id("UserName")).SendKeys(username);
            return this;
        }

        public LogonScreen enterPassword(string password)
        {
            _webDriver.FindElement(By.Id("Password")).SendKeys(password);
            _webDriver.FindElement(By.Id("Password")).SendKeys(Keys.Return);
            return this;
        }

        public StartPage SubmitLogonDetails ( string editorBaseUrl, string editorUid, string editorPassword)
        {
            navigateToLoginPage(editorBaseUrl);
            enterUsername(editorUid);
            enterPassword(editorPassword);
            return new StartPage(_webDriver);
        }
    }


}
