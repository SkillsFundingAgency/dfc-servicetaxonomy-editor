using OpenQA.Selenium;
using OpenQA.Selenium.Support.PageObjects;
using System;
using System.Collections.Generic;
using System.Text;


namespace DFC.ServiceTaxonomy.OrchardCoreInitialiser.PageObjects
{
    class AddActivity
    {
 
        //public AddActivity(ScenarioContext context)//IWebDriver driver, ScenarioContext context)
        //{
        //    _scenarioContext = context;
        //}

        //public AddActivity EnterAction (string action)
        //{
        //    _scenarioContext.GetWebDriver().FindElement(By.Id("TitlePart_Title")).SendKeys(action);
        //    return this;
           
        //}

        //public AddActivity EnterURI(string uri)
        //{
        //    _scenarioContext.GetWebDriver().FindElement(By.Id("UriId_URI_Text")).SendKeys(uri);
        //    _scenarioContext.GetWebDriver().FindElement(By.Id("UriId_URI_Text")).SendKeys(Keys.Return);
        //    return this;
        //}

        //public string GetGeneratedURI()
        //{
        //    return _scenarioContext.GetWebDriver().FindElement(By.Id("Graph_UriId_Text")).GetAttribute("value");
        //}

        //public AddActivity SetFieldValue( string field, string value)
        //{
        //    string id;
        //    switch (field)
        //    {
        //        case "Title":
        //            id = "TitlePart_Title";
        //            break;
        //        default:
        //            id = "";
        //            break;
        //    }
        //    _scenarioContext.GetWebDriver().FindElement(By.Id(id)).Clear();
        //    _scenarioContext.GetWebDriver().FindElement(By.Id(id)).SendKeys(value);
        //    return this;
        //}

        //public AddActivity PublishActivity()
        //{
        //    _scenarioContext.GetWebDriver().FindElement(By.Name("submit.Publish")).Click();
        //    //.XPath("/html/body/div[1]/div[3]/form/div[2]/div/div[3]/div/button[1]")).Click();
        //    return this;
        //}

        //public bool ConfirmSuccess()
        //{
        //    return _scenarioContext.GetWebDriver().FindElement(By.XPath("/html/body/div[1]/div[3]/div")).Text.Contains("Your Activity has been published.") ;
        //}
    }
}
