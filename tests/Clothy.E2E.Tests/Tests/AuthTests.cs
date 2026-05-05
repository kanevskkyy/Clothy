using Clothy.E2E.Tests.Pages;
using FluentAssertions;
using OpenQA.Selenium;
using Xunit;

namespace Clothy.E2E.Tests.Tests;

public class AuthTests : SeleniumBase
{
    private readonly AuthPage authPage;

    public AuthTests()
    {
        authPage = new AuthPage(this);
    }

    [Fact]
    public void Login_WithValidCredentials_RedirectsToAccount()
    {
        authPage.GoToLogin();
        authPage.Login("testuser@clothy.com", "TestPassword123!");
        WaitForUrl("/account");
        Driver.Url.Should().Contain("/account");
    }

    [Fact]
    public void Login_WithInvalidCredentials_ShowsErrorToast()
    {
        authPage.GoToLogin();
        authPage.Login("wrong@email.com", "wrongpassword");
        string toast = authPage.GetToastMessage();
        toast.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Login_WithEmptyFields_ShowsValidationErrors()
    {
        authPage.GoToLogin();
        WaitForElement(By.CssSelector("button[type='submit']"));
        Driver.FindElement(By.CssSelector("button[type='submit']")).Click();
        Driver.Url.Should().Contain("/login");
    }

    [Fact]
    public void Register_WithExistingEmail_ShowsErrorToast()
    {
        authPage.GoToRegister();
        authPage.Register("John", "Doe", "testuser@clothy.com", "+380991234567", "TestPassword123!");
        string toast = authPage.GetToastMessage();
        toast.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void LoginPage_HasLoginLink_ToRegister()
    {
        authPage.GoToLogin();
        Driver.FindElement(By.LinkText("Register")).Click();
        WaitForUrl("/register");
        Driver.Url.Should().Contain("/register");
    }
}