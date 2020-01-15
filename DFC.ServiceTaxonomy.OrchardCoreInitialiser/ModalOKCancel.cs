using DFC.ServiceTaxonomy.TestSuite.Extensions;
using DFC.ServiceTaxonomy.TestSuite.PageObjects;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.PageObjects;
using System;
using System.Collections.Generic;
using System.Text;
using TechTalk.SpecFlow;

namespace DFC.ServiceTaxonomy.TestSuite.PageObjects
{
    class ModalOKCancel
    {
        ScenarioContext _scenarioContext;
        public ModalOKCancel(ScenarioContext context)// : base(context)
        {
            _scenarioContext = context;
        }

        public void ConfirmDelete()
        {
            try
            {
                _scenarioContext.GetWebDriver().FindElement(By.Id("modalOkButton"))
                                               .Click();
            }
            catch
            {
                
            }
        }
    }
}
