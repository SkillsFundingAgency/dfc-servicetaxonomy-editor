using DFC.ServiceTaxonomy.OrchardCoreInitialiser.PageObjects;
using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;

namespace DFC.ServiceTaxonomy.OrchardCoreInitialiser
{
    class Program
    {
        static string uri = "";
        //static IWebDriver webDriver;

        static void Main(string[] args)
        {
            
            Console.WriteLine("Hello World!");
            string recipeFileLocation = 
            IWebDriver  webDriver = new ChromeDriver(Environment.CurrentDirectory);

            // Logon
            LogonScreen logonScreen = new LogonScreen(webDriver);
            StartPage startPage = logonScreen.SubmitLogonDetails(uri,
                                           $"",
                                           $"");

            // remove content types
            ContentTypesAdmin contentTypesAdmin = startPage.NavitateToContentTypeAdmin(uri);
            contentTypesAdmin.RemoveAllContentTypes();


            // load file
            webDriver.Close();
            webDriver.Quit();
            Environment.Exit(0);


        }
    }
}
