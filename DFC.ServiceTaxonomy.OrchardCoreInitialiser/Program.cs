using DFC.ServiceTaxonomy.OrchardCoreInitialiser.PageObjects;
using Mono.Options;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using System;
using System.Collections.Generic;


namespace DFC.ServiceTaxonomy.OrchardCoreInitialiser
{
    class Program
    {
        //static IWebDriver webDriver;
        static string mask = "###########################################################################################################################";

        static void Main(string[] args)
        {
            var shouldShowHelp = false;
            string uri = "https://localhost:44346/";
            string siteName = "Service Taxonomy Editor";
            string recipeName = "Service Taxonomy Editor";
            string databaseType = "Sqlite";
            string tablePrefix = "";
            string sqlConnectionString = "";
            string userName = "";
            string email = "";
            string password = "";
            bool setupAlreadyComplete = false;
            int returnCode = 0;

            var p = new OptionSet {
                { "u|uri=",    "The base url of the orchard core instance.",                u => uri = u },
                { "s|sitename=",  "The name of the site",  s => siteName = s },
                { "r|recipename=", "The name of the recipe to load during initialistation", r => recipeName = r },
                { "d|databasetype=", "The database type for the installation ( Sql Server | Sqlite | MySql | Postgres )", d => databaseType = d },
                { "t|tableprefix=", "The table prefix (not required for Sqlite )", t => tablePrefix = t },
                { "c|connectionstring=", "The sql connection string (not required for Sqlite )", c => sqlConnectionString = c },
                { "n|username=", "The default user name", n => userName = n },
                { "e|email=", "The default email address", e => email = e },
                { "p|password=", "The default password", p => password = p },
                { "x|excludesetup=", "Setup is already complete, publish only (y/n)", x => setupAlreadyComplete = (x.ToLower().Equals("y")) },
                { "h|help",     "show this message and exit",                   h => shouldShowHelp = h != null },
            };

            

            List <string> extra;
            try
            {
                // parse the command line
                extra = p.Parse(args);
            }
            catch (OptionException e)
            {
                // output some error message
                Console.Write("greet: ");
                Console.WriteLine(e.Message);
                Console.WriteLine("Try `greet --help' for more information.");
                return;
            }


            bool missingArgs = false;
            string errorMessage = "The following mandatory arguments were not supplied:\n";

            if (userName == string.Empty)
            {
                missingArgs = true;
                errorMessage += "\tuserName\n";
            }
            if (email == string.Empty)
            {
                missingArgs = true;
                errorMessage += "\temail\n";
            }
            if (password == string.Empty)
            {
                missingArgs = true;
                errorMessage += "\tpassword\n";
            }

            if ( missingArgs && !shouldShowHelp )
            {
                Console.WriteLine(errorMessage+"\n");
                shouldShowHelp = true;
            }

            if (shouldShowHelp)
            {
                ShowHelp(p);
                return;
            }


            
            Console.WriteLine("uri = " + uri);
            Console.WriteLine("siteName = " + siteName);
            Console.WriteLine("recipeName = " + recipeName);
            Console.WriteLine("databaseType = " + databaseType);
            Console.WriteLine("tablePrefix = " + tablePrefix);
            Console.WriteLine("sqlConnectionString = " + mask.Substring(1, sqlConnectionString.Length));
            Console.WriteLine("userName = " + userName);
            Console.WriteLine("email = " + email);
            Console.WriteLine("password = " + mask.Substring(1,password.Length));
            Console.WriteLine("excludeSetup = " + setupAlreadyComplete.ToString());

            IWebDriver webDriver= null;
            try
            {
                 webDriver = new ChromeDriver(Environment.CurrentDirectory);

                // setup page
                if (!setupAlreadyComplete)
                {
                    SetupPage setupPage = new SetupPage(webDriver);
                    setupPage.openPage(uri);
                    setupPage.enterSiteName(siteName);
                    setupPage.selectRecipe(recipeName);
                    setupPage.selectDatabase(databaseType);
                    if (!sqlConnectionString.Equals(string.Empty))
                        setupPage.enterConnectionString(sqlConnectionString);
                    if (!tablePrefix.Equals(string.Empty))
                        setupPage.enterTablePrefix(tablePrefix);
                    setupPage.enterUsername(userName);
                    setupPage.enterPassword(password);
                    setupPage.enterEmail(email);
                    setupPage.submitForm();
                }
                else
                    webDriver.Navigate().GoToUrl(uri);

                //// Logon
                LogonScreen logonScreen = new LogonScreen(webDriver);
                StartPage startPage = logonScreen.SubmitLogonDetails(uri, userName, password);


                //// remove content types
                //ContentTypesAdmin contentTypesAdmin = startPage.NavitateToContentTypeAdmin(uri);
                //contentTypesAdmin.RemoveAllContentTypes();


                // publish all items
                ManageContent manageContent = startPage.NavigateToManageContent(uri);
                manageContent.PublishAll("Job Category");
                manageContent.PublishAll("Job Profile");

            }
            catch( Exception e)
            {
                Console.WriteLine("An error was encountered:\n" + e);
                returnCode = -1;
            }
            finally
            {
                webDriver?.Close();
                webDriver?.Quit();
            }
            Environment.Exit(returnCode);


        }

        private static void ShowHelp(OptionSet p)
        {
            Console.WriteLine("Orchard core intialisation tool");
            Console.WriteLine("-------------------------------");
            Console.WriteLine("Arguments:");
            p.WriteOptionDescriptions(Console.Out);
        }
    }
}
