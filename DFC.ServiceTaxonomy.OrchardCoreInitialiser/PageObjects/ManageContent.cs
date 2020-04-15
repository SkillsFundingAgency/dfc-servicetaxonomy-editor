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
                selectElement.SelectByText(filter);
            }
            catch (Exception e)
            {
                throw new Exception("ManageContent: Exception in function SetFilter:\n" , e);
            }
            return this;
        }

        int GetPageRecordCount()
        {
            int items = -1;
            try
            {
                var thing = _webDriver.FindElement(By.Id("items"));
                var val = thing.Text;

                int.TryParse(val.Split(' ')[0], out items);
            }
            catch (Exception e)
            {
                throw new Exception("ManageContent: Exception in function GetPageRecordCount:\n", e);
            }
            return items;
        }
        public ManageContent PublishAll(string contentType)
        {
            SetFilter(contentType);

            int currentPage = 1;
            // assume starts on page one of results
            string baseUrl = _webDriver.Url;

            while( GetPageRecordCount() > 0)
            {
                try
                {
                    _webDriver.FindElement(By.Id("select-all")).SendKeys(" ");
                    _webDriver.FindElement(By.Id("bulk-action-menu-button")).Click();
                    _webDriver.FindElement(By.LinkText("Publish Now")).Click();
                    _webDriver.FindElement(By.Id("modalOkButton")).Click();

                    ConfirmPublishedSuccessfully();
                }
                catch (Exception e)
                {
                    throw new Exception("ManageContent: Exception in function PublishAllt:\n", e);
                }

                currentPage++;
                _webDriver.Navigate().GoToUrl(baseUrl + "&page=" + currentPage);
            }

            _webDriver.Navigate().GoToUrl(baseUrl);
            return this;
        }


        public ManageContent GetResultsPage(string filter)
        {
            return this;
        }


        public ManageContent FindItem(string title)
        {
            try
            {
                _webDriver.FindElement(By.Id("Options_DisplayText")).Clear();
                _webDriver.FindElement(By.Id("Options_DisplayText")).SendKeys(title);
                _webDriver.FindElement(By.Id("Options_DisplayText")).SendKeys(Keys.Return);
            }
            catch( Exception e)
            {
                throw new Exception("ManageContent: Exception in function FindItem:\n", e);
            }
            return this;
        }

        public ManageContent LogOut()
        {
            try
            {
                _webDriver.FindElement(By.XPath("//button[contains(.,' Log off')]")).Click();
            }
            catch (Exception e)
            {
                Console.WriteLine("ManageContent: Unable to log out:\n"+ e);
            }


            return this;
        }

        public ManageContent SelectFirstItem()
        {
            try
            {
                _webDriver.FindElement(By.XPath("//button[contains(.,' Log off')]")).Click();
            }
            catch (Exception e)
            {
                Console.WriteLine($"ManageContent: Logout failed {e}");
            }


            return this;
        }

        public ManageContent DeleteFirstItem()
        {

            try
            {
                _webDriver.FindElement(By.CssSelector(".list-group-item:nth-child(1) .btn-group > .btn"))
                                               .Click();
                _webDriver.FindElement(By.LinkText("Delete"))
                                               .Click();
            }
            catch( Exception e)
            {
                throw new Exception("ManageContent: Exception in function DeleteFirstItem: :\n", e);
            }
            return this;
        }

        public bool ConfirmRemovedSuccessfully()
        {
            return ConfirmActionSuccess(remove);
        }

        public bool ConfirmPublishedSuccessfully()
        {
            return ConfirmActionSuccess(publish);
        }

        public bool ConfirmActionSuccess(string action)
        {
            bool returnValue = false;

            try
            {
                returnValue = _webDriver.FindElement(By.XPath("/html/body/div[1]/div[3]/div")).Text
                                                             .EndsWith("has been " + action + ".");
            }
            catch( Exception e)
            {
                throw new Exception("ManageContent: Exception in function ConfirmActionSuccess:\n", e);
            }
            return returnValue;
        }


    }
}
