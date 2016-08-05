using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support;
using OpenQA.Selenium.Support.UI;
using System.Collections.ObjectModel;
using OpenQA.Selenium.Interactions;
using System.Threading;

namespace ZapSelenium
{
    class Program
    {
        static void Main(string[] args)
        {
            ZapModal.InitDriver();
            ZapModal.GetResults();

        }
    }

    public static class ZapModal
    {
        static IWebDriver driver;
        static string chromeDriverDirectory = "C:\\";
        static string url = "http://www.zapimoveis.com.br/";
        static int waitTimeInSec = 60;
        static Actions actions;
        static string selEstadoModalLocalidade = "#s2id_txtEstadoModalLocalidade .select2-choice";
        static string selCidadeModalLocalidade = "#s2id_txtCidadeModalLocalidade .select2-choice";
        static string selLocalidadeModalLocalidade = "#s2id_txtLocalidadeModalLocalidade .select2-choice";
        static string selZonaModalLocalidade = "#s2id_txtZonaModalLocalidade .select2-choice";
        static string selDropDownValues = ".select2-results .select2-results-dept-0";

        public static void InitDriver()
        {
            driver = new ChromeDriver(chromeDriverDirectory);
            driver.Navigate().GoToUrl(url);
            actions = new Actions(driver);
        }

        public static void GetResults()
        {
            Thread.Sleep(10000);
            //open modal
            driver.FindElement(By.Id("btnQtdeLocalidadeHome"), waitTimeInSec).Click();
            Sleep1000();

            RecursiveDropdown(1);
        }
       
        private static void RecursiveDropdown(int position)
        {
            string selector = GetSelector(position);

            if (!string.IsNullOrEmpty(selector))
            {
                CustomClickBySelector(selector);
                Sleep1000();

                var isOpen = true;
                foreach (var v in GetValuesBySelector(selDropDownValues))
                {
                    Sleep1000();
                    if (!isOpen)
                    {
                        CustomClickBySelector(selector);
                        Sleep1000();
                    }

                    driver.FindElementsByJsWithWait(GetContainsScript(v), waitTimeInSec).First().Click();
                    Sleep1000();
                    isOpen = false;

                    RecursiveDropdown(position + 1);
                }
            }
        }

        private static string GetSelector(int nivel)
        {
            var selector = "";

            switch (nivel)
            {
                case 1:
                    selector = selEstadoModalLocalidade;
                    break;
                case 2:
                    if (IsElementDisplayedBySelector(selCidadeModalLocalidade))
                        selector = selCidadeModalLocalidade;
                    else if (IsElementDisplayedBySelector(selLocalidadeModalLocalidade))
                        selector = selLocalidadeModalLocalidade;
                    break;
                case 3:
                    if (IsElementDisplayedBySelector(selZonaModalLocalidade))
                        selector = selZonaModalLocalidade;
                    else if (IsElementDisplayedBySelector(selCidadeModalLocalidade))
                        selector = selCidadeModalLocalidade;
                    break;
            }

            return selector;
        }

        public static void Sleep1000()
        {
            Thread.Sleep(1000);
        }

        public static bool IsElementDisplayedBySelector(string selector)
        {
            return driver.IsElementDisplayed(By.CssSelector(selector));
        }

        public static List<string> GetValuesBySelector(string selector)
        {
            return driver.FindElements(By.CssSelector(selector), waitTimeInSec).Select(d => d.Text).ToList();
        }

        public static string GetContainsScript(string value)
        {
            return string.Format("return $('.select2-results .select2-results-dept-0 .select2-result-label:contains({0})')", value);

        }

        public static void CustomClickBySelector(string selector)
        {
            CustomClick(driver.FindElement(By.CssSelector(selector), waitTimeInSec));
        }

        public static void CustomClick(IWebElement element)
        {
            actions.Click(element).Build().Perform();
        }

    }


    public static class WebDriverExtensions
    {
        public static bool IsElementDisplayed(this IWebDriver driver, By element)
        {
            if (driver.FindElements(element).Count > 0)
            {
                if (driver.FindElement(element).Displayed)
                    return true;
                else
                    return false;
            }
            else
            {
                return false;
            }
        }

        public static bool IsElementEnabled(this IWebDriver driver, By element)
        {
            if (driver.FindElements(element).Count > 0)
            {
                if (driver.FindElement(element).Enabled)
                    return true;
                else
                    return false;
            }
            else
            {
                return false;
            }
        }

        public static IWebElement FindElementByJs(this IWebDriver driver, string jsCommand)
        {
            return (IWebElement)((IJavaScriptExecutor)driver).ExecuteScript(jsCommand);
        }

        public static ReadOnlyCollection<IWebElement> FindElementsByJs(this IWebDriver driver, string jsCommand)
        {
            return (ReadOnlyCollection<IWebElement>)((IJavaScriptExecutor)driver).ExecuteScript(jsCommand);
        }


        public static ReadOnlyCollection<IWebElement> FindElementsByJsWithWait(this IWebDriver driver, string jsCommand, int timeoutInSeconds)
        {
            if (timeoutInSeconds > 0)
            {
                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutInSeconds));
                return wait.Until(drv => (drv.FindElementsByJs(jsCommand).Count > 0) ? drv.FindElementsByJs(jsCommand) : null);
            }
            return driver.FindElementsByJs(jsCommand);
        }

        public static IWebElement FindElementByJsWithWait(this IWebDriver driver, string jsCommand, int timeoutInSeconds)
        {
            if (timeoutInSeconds > 0)
            {
                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutInSeconds));
                wait.Until(d => d.FindElementByJs(jsCommand));
            }
            return driver.FindElementByJs(jsCommand);
        }

        public static IWebElement FindElement(this IWebDriver driver, By by, int timeoutInSeconds)
        {
            if (timeoutInSeconds > 0)
            {
                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutInSeconds));
                return wait.Until(drv => drv.FindElement(by));
            }
            return driver.FindElement(by);
        }

        public static ReadOnlyCollection<IWebElement> FindElements(this IWebDriver driver, By by, int timeoutInSeconds)
        {
            if (timeoutInSeconds > 0)
            {
                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutInSeconds));
                return wait.Until(drv => (drv.FindElements(by).Count > 0) ? drv.FindElements(by) : null);
            }
            return driver.FindElements(by);
        }
    }

    public class ZapResults
    {
        public string Key { get; set; }
        public string Result { get; set; }
        public List<ZapResults> Childrens { get; set; }
    }
}
