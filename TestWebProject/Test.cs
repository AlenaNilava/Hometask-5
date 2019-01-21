namespace TestWebProject
{
    using System;
    using System.Configuration;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Firefox;
    using OpenQA.Selenium.Support.UI;
    using OpenQA.Selenium.Chrome;

    [TestClass]
    public class Test
	{
		private IWebDriver driver;
		private string baseUrl;

		private By draftEmail = By.XPath(String.Format("(//div[text()='{0}'])[1]", expectedSubject));
        private By sentEmail = By.XPath(String.Format("(//a[@data-subject='{0}'])[1]", expectedSubject));
        private By fieldTo = By.XPath("//textarea[@data-original-name='To']");
        private By to = By.XPath("(//span[text()='elenasinevich91@gmail.com'])[2]");
        private By subject = By.XPath("//input[@name='Subject']");
        private By body = By.XPath("(//span[text()='Test Text -- Елена Нилова'])[1]");
        private By draftBody = By.XPath("//body[@id='tinymce']//div[text()[contains(.,'Test Text')]]");
        private By submit = By.XPath("//*[@id='mailbox:submit']");
        private By create = By.XPath("(//span[text()='Написать письмо'])[1]");
        private By draftFolder = By.XPath("//*[@data-mnemo='drafts']");

        string expectedTo = "elenasinevich91@gmail.com";
        static string expectedSubject = String.Format("Test Mail {0}", GetRandomSubjectNumber());
        string expectedTestBody = "Test Text";


        private string configValue = ConfigurationManager.AppSettings["browser"];

		[TestInitialize]
		public void SetupTest()
		{
			if ("ff".Equals(this.configValue))
			{
				var service = FirefoxDriverService.CreateDefaultService();
				this.driver = new FirefoxDriver(service);
			}
			else
			{
				ChromeOptions option = new ChromeOptions();
				option.AddArgument("disable-infobars");
				this.driver = new ChromeDriver(option);
			}

            // Set up implicit wait
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);

            this.baseUrl = "https://mail.ru/";

			this.driver.Navigate().GoToUrl(this.baseUrl);
            this.driver.Manage().Window.Maximize();
		}

		[TestMethod]
		public void TestSmokeEmail()
		{
			//Enter login
			this.driver.FindElement(By.Id("mailbox:login")).SendKeys("testmail.2020");

            //Select '@mail.ru' domain
            this.driver.FindElement(By.Id("mailbox:domain")).Click();
     
            // Enter password
            this.driver.FindElement(By.Id("mailbox:password")).SendKeys("Asas432111");

            // Click Enter button
            this.driver.FindElement(submit).Click();

            // Verifying that Create button is visible and click it
			IsElementVisible(create);
            this.driver.FindElement(create).Click();

            //Verifying link 'Adding file' is visible
            IsElementVisible(By.XPath("//*[text()='Прикрепить файл']"));

            //Enter addressee
            driver.FindElement(By.XPath("//textarea[@data-original-name='To']")).SendKeys("elenasinevich91@gmail.com");

            //Enter Subject of the mail
            this.driver.FindElement(By.Name("Subject")).SendKeys(expectedSubject);

            // Switching to frame
            this.driver.SwitchTo().Frame(this.driver.FindElement(By.XPath("//iframe")));
            
            //Enter body text
            this.driver.FindElement(By.XPath("//*[@id='tinymce']")).SendKeys(expectedTestBody);
            this.driver.SwitchTo().DefaultContent();
            
            //Click Save button and wait the text 'Сохранено' is visible on the screen
            this.driver.FindElement(By.XPath("(//div[@data-name='saveDraft'])[1]")).Click();
            IsElementVisible(By.XPath("(//div[text()='Сохранено в '])[1]"));

            //Click Drafts folder
            IsElementAvailable(draftFolder);
            this.driver.FindElement(draftFolder).Click();
           
            //Comented as implicit wait was used for FindElement method
            //IsElementVisible(draftEmail);
            
            //Click on the first email in the draft folder list
            this.driver.FindElement(draftEmail).Click();

            //Verify the draft addressee is still the same
            IsElementVisible(fieldTo);
            this.driver.FindElement(fieldTo);
            Assert.AreEqual(expectedTo, this.driver.FindElement(to).Text, "Email is not actual");

            //Verify the draft subject is still the same
            Assert.AreEqual(expectedSubject, this.driver.FindElement(subject).GetAttribute("value"), "Subject is not actual");

            //Verify the draft body text is still the same
            this.driver.SwitchTo().Frame(this.driver.FindElement(By.XPath("//iframe")));
            Assert.IsTrue(this.driver.FindElement(draftBody).Text.Contains(expectedTestBody), "Body text is not as expected"); ;
            this.driver.SwitchTo().DefaultContent();
        
            //Click Send button
            this.driver.FindElement(By.XPath("(//div[@data-name='send'])[1]")).Click();

            //Wait for element "Ваше письмо отправлено" is visible on the screen
            IsElementVisible(By.XPath("(//div[@class='message-sent__title'])[1]"));

            //Go to Drafts folder again
            IsElementAvailable(By.XPath("//*[@data-mnemo='drafts']"));
            this.driver.FindElement(By.XPath("//*[@data-mnemo='drafts']")).Click();

            //Verify that the mail disappeared from Drafts folder
            IsElementNotVisible(sentEmail, 15);

            //Go to Sent folder
            this.driver.FindElement(By.XPath("//span[text()='Отправленные']")).Click();
            
            //Verify that the mail appeared in the Sent folder 
            IsElementVisible(sentEmail);

            //Log out
            this.driver.FindElement(By.XPath("//a[@id='PH_logoutLink']")).Click();
            IsElementVisible(submit);

        }

        [TestCleanup]
		public void CleanUp()
		{
			this.driver.Close();
			this.driver.Quit();
		}

		public void IsElementVisible(By element, int timeoutSecs = 10)
		{
			new WebDriverWait(this.driver, TimeSpan.FromSeconds(timeoutSecs)).Until(ExpectedConditions.ElementIsVisible(element));
		}

        public void IsElementNotVisible(By element, int timeoutSecs = 10)
        {
            new WebDriverWait(this.driver, TimeSpan.FromSeconds(timeoutSecs)).Until(ExpectedConditions.InvisibilityOfElementLocated(element));
        }

        //catch Stale exception due elements loading
        public void IsElementAvailable(By element, int timeoutSecs = 10)
        {
            try
            {
                new WebDriverWait(this.driver, TimeSpan.FromSeconds(timeoutSecs)).Until(ExpectedConditions.PresenceOfAllElementsLocatedBy(element));
                new WebDriverWait(this.driver, TimeSpan.FromSeconds(timeoutSecs)).Until(ExpectedConditions.ElementToBeClickable(element));
            }
            catch (StaleElementReferenceException e)
            {
                new WebDriverWait(this.driver, TimeSpan.FromSeconds(timeoutSecs)).Until(ExpectedConditions.PresenceOfAllElementsLocatedBy(element));
                new WebDriverWait(this.driver, TimeSpan.FromSeconds(timeoutSecs)).Until(ExpectedConditions.ElementToBeClickable(element));
            }
        }

        public void JavaScriptClick(IWebElement element)
		{
			IJavaScriptExecutor executor = (IJavaScriptExecutor)driver;
			executor.ExecuteScript("arguments[0].click();", element);
		}

        public static string GetRandomSubjectNumber()
        {
            return Guid.NewGuid().ToString("N");
        }
	}
}
