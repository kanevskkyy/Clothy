using Clothy.E2E.Tests.Pages;
using FluentAssertions;
using OpenQA.Selenium;
using Xunit;

namespace Clothy.E2E.Tests.Tests;

public class CheckoutTests : SeleniumBase
{
    private readonly AuthPage authPage;
    private readonly CheckoutPage checkoutPage;
    private readonly CartPage cartPage;

    public CheckoutTests()
    {
        authPage = new AuthPage(this);
        checkoutPage = new CheckoutPage(this);
        cartPage = new CartPage(this);
    }

    private void LoginAsTestUser()
    {
        authPage.GoToLogin();
        authPage.Login("testuser@clothy.com", "TestPassword123!");
        WaitForUrl("/account");
    }

    [Fact]
    public void Checkout_WithEmptyCart_RedirectsToCatalog()
    {
        LoginAsTestUser();
        cartPage.GoToCart();
        if (!cartPage.IsCartEmpty()) cartPage.ClickClearAll();
        checkoutPage.GoToCheckout();
        WaitForUrl("/catalog");
        Driver.Url.Should().Contain("/catalog");
    }

    [Fact]
    public void Checkout_PageLoads_HasPersonalInfoSection()
    {
        LoginAsTestUser();
        cartPage.GoToCart();
        if (cartPage.GetCartItemCount() == 0) return;
        cartPage.ClickCheckout();

        WaitForElement(By.Id("firstName"));
        checkoutPage.IsOnCheckoutPage().Should().BeTrue();
        Driver.FindElement(By.XPath("//*[contains(text(),'Personal information')]")).Should().NotBeNull();
    }

    [Fact]
    public void Checkout_HasPaymentOptions_CardAndCrypto()
    {
        LoginAsTestUser();
        cartPage.GoToCart();
        if (cartPage.GetCartItemCount() == 0) return;
        cartPage.ClickCheckout();

        WaitForElement(By.Id("payment-card"));
        ElementExists(By.Id("payment-card")).Should().BeTrue();
        ElementExists(By.Id("payment-crypto")).Should().BeTrue();
    }

    [Fact]
    public void Checkout_SelectCryptoPayment_UpdatesSelection()
    {
        LoginAsTestUser();
        cartPage.GoToCart();
        if (cartPage.GetCartItemCount() == 0) return;
        cartPage.ClickCheckout();

        WaitForElement(By.Id("payment-crypto"));
        checkoutPage.SelectPaymentCrypto();

        IWebElement crypto = Driver.FindElement(By.Id("payment-crypto"));
        crypto.Selected.Should().BeTrue();
    }

    [Fact]
    public void Checkout_ClickPlaceOrder_WithEmptyFields_StaysOnPage()
    {
        LoginAsTestUser();
        cartPage.GoToCart();
        if (cartPage.GetCartItemCount() == 0) return;
        cartPage.ClickCheckout();

        WaitForElement(By.Id("firstName"));
        IWebElement fn = Driver.FindElement(By.Id("firstName"));
        fn.Clear();
        checkoutPage.ClickPlaceOrder();
        Driver.Url.Should().Contain("/checkout");
    }

    [Fact]
    public void Checkout_HasWarningAbout10Minutes()
    {
        LoginAsTestUser();
        cartPage.GoToCart();
        if (cartPage.GetCartItemCount() == 0) return;
        cartPage.ClickCheckout();

        WaitForElementByText("10 minutes");
        checkoutPage.GetOrderWarningText().Should().Contain("10 minutes");
    }
}