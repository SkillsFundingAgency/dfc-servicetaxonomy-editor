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
        static void Main(string[] args)
        {
            var shouldShowHelp = false;
            string uri = "https://localhost:44346";
            string siteName = "Service Taxonomy Editor";
            string recipeName = "Service Taxonomy Editor";
            string databaseType = "";
            string tablePrefix = "";
            string sqlConnectionString = "";
            string userName = "";
            string email = "";
            string password = "";
            bool runPublishIfAlreadySetUp = false;
            bool alreadySetup = false;
            int returnCode = 0;

            var p = new OptionSet {
                { "u|uri=",    "The base url of the orchard core instance.",                u => uri = u },
                { "s|sitename=",  "The name of the site",  s => siteName = s },
                { "r|recipename=", "The name of the recipe to load during initialistation", r => recipeName = r },
                { "d|databasetype=", "The database type for the installation ( SqlConnection | Sqlite | MySql | Postgres )", d => databaseType = d },
                { "t|tableprefix=", "The table prefix (not required for Sqlite )", t => tablePrefix = t },
                { "c|connectionstring=", "The sql connection string (not required for Sqlite )", c => sqlConnectionString = c },
                { "n|username=", "The default user name", n => userName = n },
                { "e|email=", "The default email address", e => email = e },
                { "p|password=", "The default password", p => password = p },
                { "a|alwaysRunPublish=", "If Setup is already complete, publish anyway (y/n)", x => runPublishIfAlreadySetUp = (x.ToLower().Equals("y")) },
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

            if (!uri.EndsWith("/"))
            {
                Console.WriteLine("Adding trailing slash to uri");
                uri += "/";
            }
            
            Console.WriteLine("uri = " + uri);
            Console.WriteLine("siteName = " + siteName);
            Console.WriteLine("recipeName = " + recipeName);
            Console.WriteLine("databaseType = " + databaseType);
            Console.WriteLine("tablePrefix = " + tablePrefix);
            Console.WriteLine("sqlConnectionString = " + new String('#',sqlConnectionString.Length));
            Console.WriteLine("userName = " + userName);
            Console.WriteLine("email = " + email);
            Console.WriteLine("password = " + new String('#', password.Length));
            Console.WriteLine("runPublishIfAlreadySetUp = " + runPublishIfAlreadySetUp.ToString());

            IWebDriver webDriver= null;
            try
            {
                ChromeOptions options = new ChromeOptions();
                if (Environment.GetEnvironmentVariable("SYSTEM_TEAMFOUNDATIONCOLLECTIONURI") == "https://sfa-gov-uk.visualstudio.com/")
                {
                    Console.WriteLine("Running on build server, using headless browser");
                    options.AddArgument("--headless");
                }
                
                webDriver = new ChromeDriver(Environment.CurrentDirectory, options);

                // setup page

                SetupPage setupPage = new SetupPage(webDriver);
                setupPage.openPage(uri);
                alreadySetup = !setupPage.verifyPageLoaded();

                if (!alreadySetup)
                {
                    Console.WriteLine("Initial configuration not completed, configuring site and database");
                    setupPage.enterSiteName(siteName);
                    setupPage.selectRecipe(recipeName);
                    if (!databaseType.Equals(string.Empty))
                    {
                        Console.WriteLine(String.Format("Setting database type to {0}", databaseType));
                        setupPage.selectDatabase(databaseType);
                    }
                    if (!sqlConnectionString.Equals(string.Empty))
                    {
                        Console.WriteLine("Setting SQL Connection String");
                        setupPage.enterConnectionString(sqlConnectionString);
                    }
                    if (!tablePrefix.Equals(string.Empty))
                        setupPage.enterTablePrefix(tablePrefix);
                    setupPage.enterUsername(userName);
                    setupPage.enterPassword(password);
                    setupPage.enterEmail(email);

                    setupPage.submitForm();
                    if (!setupPage.checkSubmissionSuccess())
                    {
                        throw new Exception("Setup did not complete succesfully");
                    };
                }
                else
                {
                    Console.WriteLine(string.Format("Initial configuration already completed, navigating to {0}", uri));
                    webDriver.Navigate().GoToUrl(uri);
                }
                    

                //// Logon
                Console.WriteLine("Initial setup complete, logging on ...");
                LogonScreen logonScreen = new LogonScreen(webDriver);
                StartPage startPage = logonScreen.SubmitLogonDetails(uri, userName, password);

                if (!alreadySetup || runPublishIfAlreadySetUp)
                {
                    //// remove content types
                    //ContentTypesAdmin contentTypesAdmin = startPage.NavitateToContentTypeAdmin(uri);
                    //contentTypesAdmin.RemoveAllContentTypes();

                    // publish all items
                    Console.WriteLine("Logged on, starting Service Taxonomy Editor configuration ...");
                    ManageContent manageContent = startPage.NavigateToManageContent(uri);
                    manageContent.PublishAll("Job Category");
                    manageContent.PublishAll("Job Profile");
                    manageContent.LogOut();
                }
                else
                {
                    startPage.LogOut();
                }



            }
            catch( Exception e)
            {
                Console.WriteLine("An error was encountered:\n" + e);
                if (webDriver != null)
                {
                    String filepath = String.Format("{0}/OrchardCoreSetupScreenshot-{1}.png", Environment.CurrentDirectory, DateTime.Now.ToString("yyyyMMdd-HHmm"));
                    Console.WriteLine(String.Format("saving screenshot to {0}", filepath));
                    Screenshot screenShot = ((ITakesScreenshot)webDriver).GetScreenshot();
                    screenShot.SaveAsFile(filepath);
                }
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
