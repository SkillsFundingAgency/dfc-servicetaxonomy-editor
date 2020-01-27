﻿using OpenQA.Selenium;
using OpenQA.Selenium.Support.PageObjects;
using System;
using System.Collections.Generic;
using System.Text;



namespace DFC.ServiceTaxonomy.OrchardCoreInitialiser.PageObjects
{
    class StartPage
    {
        private IWebDriver _webDriver;
        //EnvironmentSettings env = new EnvironmentSettings();
        //private ScenarioContext _scenarioContext;

        public StartPage(IWebDriver driver)
        {
            _webDriver = driver;

        }

        public StartPage NavigateTo( string sBaseUrl, string sPath)
        {
            _webDriver.Navigate().GoToUrl(sBaseUrl + sPath);
            return this;
        }

        //public ContentTypesAdmin NavitateToContentTypeAdmin(string sBaseUrl)
        //{
        //    NavigateTo(sBaseUrl, "OrchardCore.ContentTypes/Admin/List");
        //    return new ContentTypesAdmin(_webDriver);
        //}

        public ManageContent NavigateToManageContent(string sBaseUrl)
        {
            NavigateTo(sBaseUrl, "Admin/Contents/ContentItems");
            return new ManageContent(_webDriver);
        }

        public StartPage LogOut()
        {
            try
            {
                _webDriver.FindElement(By.XPath("//button[contains(.,' Log off')]")).Click();
            }
            catch (Exception e)
            {
                Console.WriteLine("StartPage: Unable to logout\n" + e);
            }
            return this;
        }

        //public AddActivity NavigateToNewActivity()
        //{
        //    _scenarioContext.GetWebDriver().Url = _scenarioContext.GetEnv().editorBaseUrl + "/Admin/Contents/ContentTypes/Activity/Create";
        //    return new AddActivity (_scenarioContext);
        //}

    }
}
