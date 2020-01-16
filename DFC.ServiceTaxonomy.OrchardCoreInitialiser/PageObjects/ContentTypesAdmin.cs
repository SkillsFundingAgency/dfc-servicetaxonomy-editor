using OpenQA.Selenium;
using OpenQA.Selenium.Support.PageObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace DFC.ServiceTaxonomy.OrchardCoreInitialiser.PageObjects
{
    class ContentTypesAdmin
    {
        private IWebDriver _webDriver;
        //EnvironmentSettings env = new EnvironmentSettings();
        //private ScenarioContext _scenarioContext;

        public ContentTypesAdmin(IWebDriver driver)
        {
            _webDriver = driver;

        }

        public void RemoveAllContentTypes()
        {
            bool carryOn = true;
            int workload = 0;
            int count = 0;

            do
            {
                var editButtons = _webDriver.FindElements(By.LinkText("Edit"));
                workload = editButtons.Count;

                if (workload > 0)
                {
                    editButtons[0].Click();
                    _webDriver.FindElement(By.LinkText("Delete")).Click();
                    _webDriver.Navigate().Back();
                }
                else
                    carryOn = false;


            }
            while (carryOn);


            

            //foreach ( var btn in editButtons)
            //{
            //    btn.Click();
            //    _webDriver.Navigate().Back();
            //    //_webDriver.FindElement(By.LinkText("Delete")).Click();

            //}


        }
    }
}
