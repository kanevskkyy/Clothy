using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace Clothy.E2E.Tests;

public class SeleniumBase : IDisposable
{
    public IWebDriver Driver;
    public WebDriverWait Wait;

    public string BaseUrl =
        Environment.GetEnvironmentVariable("FRONTEND__URL")?.Split(",").First()
        ?? "http://localhost:3000";

    protected SeleniumBase()
    {
        ChromeOptions options = new ChromeOptions();
        options.AddArgument("--headless");
        options.AddArgument("--no-sandbox");
        options.AddArgument("--disable-dev-shm-usage");
        options.AddArgument("--window-size=1920,1080");

        Driver = new ChromeDriver(options);
        Wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(15));
        Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
    }

    public void GoTo(string path) => Driver.Navigate().GoToUrl($"{BaseUrl}{path}");

    public IWebElement ById(string id) => Driver.FindElement(By.Id(id));

    public IWebElement ByCss(string css) => Driver.FindElement(By.CssSelector(css));

    public IReadOnlyCollection<IWebElement> AllByCss(string css) => Driver.FindElements(By.CssSelector(css));

    public void WaitForUrl(string urlPart, int seconds = 15)
    {
        WebDriverWait wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(seconds));
        wait.Until(d => d.Url.Contains(urlPart));
    }

    public void WaitForElement(By by) => Wait.Until(d => d.FindElement(by));

    public void WaitForElementByText(string text) =>
        Wait.Until(d => d.FindElements(By.XPath($"//*[contains(text(),'{text}')]")).Count > 0);

    public bool ElementExists(By by)
    {
        try { Driver.FindElement(by); return true; }
        catch (NoSuchElementException) { return false; }
    }

    public void Dispose()
    {
        try { Driver.Quit(); Driver.Dispose(); }
        catch { }
    }
}