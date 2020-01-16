using DFC.ServiceTaxonomy.OrchardCoreInitialiser.PageObjects;
using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;

namespace DFC.ServiceTaxonomy.OrchardCoreInitialiser
{
    class Program
    {
        //static IWebDriver webDriver;

        static void Main(string[] args)
        {
            string uri = "https://localhost:44346/";
            string siteName = "Site Name";
            string recipeName = "Service Taxonomy Editor";
            string databaseType = "Sqlite";
            string tablePrefix = "";
            string sqlConnectionString = "";
            string userName = "admin";
            string email = "ad@mi.n";
            string password = "Password1!";

            ///siteName:'Site Name' /recipeName:'Service Taxonomy Editor' /databaseType:Sqlite /userName:admin /email:ad@mi.n /password:Password1!
            // handle args
            int count = 0;
            int expectedParams = 6;

            foreach (var arg in args)
            {
                if (arg.StartsWith("/siteName:"))
                {
                    siteName = arg.Split(':')[1];
                }
                else if (arg.StartsWith("/recipeName:"))
                {
                    recipeName = arg.Split(':')[1];
                }
                else if (arg.StartsWith("/databaseType:"))
                {
                    databaseType = arg.Split(':')[1];
                }
                else if (arg.StartsWith("/userName:"))
                {
                    userName = arg.Split(':')[1];
                }
                else if (arg.StartsWith("/email:"))
                {
                    email = arg.Split(':')[1];
                }
                else if (arg.StartsWith("/password:"))
                {
                    password = arg.Split(':')[1];
                }
                else
                {
                    throw (new NotSupportedException(string.Format("Argument: {0} is invalid", arg)));
                }
                count++;
            }

            if ( count != expectedParams )
            {
                throw (new NotSupportedException(string.Format("Arguments: Not all expected arguments supplied.")));
            }

            IWebDriver  webDriver = new ChromeDriver(Environment.CurrentDirectory);

            // setup page
            SetupPage setupPage = new SetupPage(webDriver);
            setupPage.openPage(uri);
            setupPage.enterSiteName(siteName);
            setupPage.selectRecipe(recipeName);
            setupPage.selectDatabase(databaseType);
            setupPage.enterUsername(userName);
            setupPage.enterPassword(password);
            setupPage.enterEmail(email);
            setupPage.submitForm();




            //// Logon
            LogonScreen logonScreen = new LogonScreen(webDriver);
            StartPage startPage = logonScreen.SubmitLogonDetails(uri, userName, password);


            //// remove content types
            //ContentTypesAdmin contentTypesAdmin = startPage.NavitateToContentTypeAdmin(uri);
            //contentTypesAdmin.RemoveAllContentTypes();


            // publish all items
            ManageContent  manageContent = startPage.NavigateToManageContent(uri);
            manageContent.PublishAll("Activity");


            webDriver.Close();
            webDriver.Quit();
            Environment.Exit(0);


        }
    }
}
