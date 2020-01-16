using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium.Support.PageObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace DFC.ServiceTaxonomy.OrchardCoreInitialiser.PageObjects
{
    class ManageContent// : PageBase
    {
        #region constants
        const string remove = "removed";
        const string publish = "published";

        #endregion

        IWebDriver _webDriver;
        public ManageContent(IWebDriver driver)
        {
            _webDriver = driver;
        }

        public ManageContent SetFilter(string filter)
        {
            try
            {
                var databaseList = _webDriver.FindElement(By.Id("Options_SelectedContentType"));
                var selectElement = new SelectElement(databaseList);
                selectElement.SelectByValue(filter);
            }
            catch (Exception e)
            {

            }
            return this;
        }

        public ManageContent PublishAll(string contentType)
        {
            SetFilter(contentType);

            int currentPage = 1;
            // assume starts on page one of results
            string baseUrl = _webDriver.Url;





            return this;
        }


        public ManageContent GetResultsPage(string filter)
        {
            return this;
        }
        public ManageContent BulkAction()
        {
            // if results on page

            // select all

            // bulk actions--publish

            // increment target page

            // check if new page is valid



            try
            {
                var databaseList = _webDriver.FindElement(By.Id("Options_SelectedContentType"));
                var selectElement = new SelectElement(databaseList);
              //  selectElement.SelectByValue(filter);
            }
            catch (Exception e)
            {

            }
            return this;
        }

        //public ManageContent FindItem( string title)
        //{
        //    _scenarioContext.GetWebDriver().FindElement(By.Id("Options_DisplayText")).Clear();
        //    _scenarioContext.GetWebDriver().FindElement(By.Id("Options_DisplayText")).SendKeys(title);
        //    _scenarioContext.GetWebDriver().FindElement(By.Id("Options_DisplayText")).SendKeys(Keys.Return);
        //    return this;
        //}

        //public ManageContent SelectFirstItem()
        //{
        //    _scenarioContext.GetWebDriver().FindElement(By.ClassName("list-group-item"))
        //                                   .FindElement(By.LinkText("Edit"))
        //                                   .Click();
        //    //// RemoteWebElement list  _scenarioContext.GetWebDriver().FindElements(By.ClassName("list-group"));
        //    //IWebElement element  = _scenarioContext.GetWebDriver().FindElement(By.ClassName("list-group-item"));

        //    //try
        //    //{
        //    //    IWebElement element2 = element.FindElement(By.LinkText("Edit"));
        //    //    element.FindElement(By.LinkText("Edit")).Click();
        //    //}
        //    //catch
        //    //{

        //    //}
        //    ////.FindElement(By.ClassName("btn btn - primary btn - sm")).Click();
        //    return this;
        //}

        //public ManageContent DeleteFirstItem()
        //{

        //    try
        //    {


        //        _scenarioContext.GetWebDriver().FindElement(By.CssSelector(".list-group-item:nth-child(1) .btn-group > .btn"))
        //                                       .Click();
        //        _scenarioContext.GetWebDriver().FindElement(By.LinkText("Delete"))
        //                                       .Click();
        //    }
        //    catch
        //    {

        //    }
        //    return this;
        //}

        //public bool ConfirmRemovedSuccessfully()
        //{
        //    return ConfirmActionSuccess(remove);
        //}

        //public bool ConfirmPublishedSuccessfully()
        //{
        //    return ConfirmActionSuccess(publish);
        //}

        //public bool ConfirmActionSuccess(string action)
        //{
        //    bool returnValue = false;

        //    try
        //    {
        //        returnValue = _scenarioContext.GetWebDriver().FindElement(By.XPath("/html/body/div[1]/div[3]/div")).Text
        //                                                     .EndsWith("has been " + action + ".");
        //    }
        //    catch
        //    {

        //    }
        //    return returnValue;
        //}


    }
}
