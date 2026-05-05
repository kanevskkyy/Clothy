using OpenQA.Selenium;

namespace Clothy.E2E.Tests.Pages;

public class AuthPage
{
    private readonly SeleniumBase ctx;

    public AuthPage(SeleniumBase ctx) => this.ctx = ctx;

    public void GoToLogin() => ctx.GoTo("/login");
    public void GoToRegister() => ctx.GoTo("/register");

    public void Login(string email, string password)
    {
        ctx.WaitForElement(By.Id("email"));
        ctx.ById("email").Clear();
        ctx.ById("email").SendKeys(email);
        ctx.ById("password").Clear();
        ctx.ById("password").SendKeys(password);
        ctx.Driver.FindElement(By.CssSelector("button[type='submit']")).Click();
    }

    public void Register(string firstName, string lastName, string email, string phone, string password)
    {
        ctx.WaitForElement(By.Id("firstName"));
        ctx.ById("firstName").SendKeys(firstName);
        ctx.ById("lastName").SendKeys(lastName);
        ctx.ById("email").SendKeys(email);
        ctx.ById("phoneNumber").SendKeys(phone);
        ctx.ById("password").SendKeys(password);
        ctx.Driver.FindElement(By.CssSelector("button[type='submit']")).Click();
    }

    public bool IsLoggedIn() => ctx.Driver.Url.Contains("/account") || ctx.Driver.Url.Contains("/admin");

    public string GetToastMessage()
    {
        ctx.WaitForElement(By.CssSelector("[data-sonner-toast]"));
        return ctx.ByCss("[data-sonner-toast]").Text;
    }

    public void Logout() => ctx.GoTo("/account");
}