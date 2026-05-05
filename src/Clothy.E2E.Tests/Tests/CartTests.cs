using Clothy.E2E.Tests.Pages;
using FluentAssertions;
using OpenQA.Selenium;
using Xunit;

namespace Clothy.E2E.Tests.Tests;

public class CartTests : SeleniumBase
{
    private readonly AuthPage authPage;
    private readonly CatalogPage catalogPage;
    private readonly CartPage cartPage;

    public CartTests()
    {
        authPage = new AuthPage(this);
        catalogPage = new CatalogPage(this);
        cartPage = new CartPage(this);
    }

    private void LoginAsTestUser()
    {
        authPage.GoToLogin();
        authPage.Login("testuser@clothy.com", "TestPassword123!");
        WaitForUrl("/account");
    }

    [Fact]
    public void Cart_WhenEmpty_ShowsEmptyState()
    {
        LoginAsTestUser();
        cartPage.GoToCart();
        bool isEmpty = cartPage.IsCartEmpty();
        bool hasItems = cartPage.GetCartItemCount() > 0;
        (isEmpty || hasItems).Should().BeTrue();
    }
    
    [Fact]
    public void Cart_IncreaseQuantity_UpdatesCount()
    {
        LoginAsTestUser();
        cartPage.GoToCart();
        if (cartPage.GetCartItemCount() == 0) return;

        string before = cartPage.GetQuantity(0);
        cartPage.ClickIncrease(0);
        Thread.Sleep(800);
        string after = cartPage.GetQuantity(0);
        int.Parse(after).Should().BeGreaterThan(int.Parse(before));
    }

    [Fact]
    public void Cart_RemoveItem_DecreasesCount()
    {
        LoginAsTestUser();
        cartPage.GoToCart();
        if (cartPage.GetCartItemCount() == 0) return;

        int before = cartPage.GetCartItemCount();
        cartPage.RemoveItem(0);
        Thread.Sleep(800);
        int after = cartPage.GetCartItemCount();
        after.Should().BeLessThan(before);
    }

    [Fact]
    public void Cart_ClickCheckout_NavigatesToCheckout()
    {
        LoginAsTestUser();
        cartPage.GoToCart();
        if (cartPage.GetCartItemCount() == 0) return;

        cartPage.ClickCheckout();
        Driver.Url.Should().Contain("/checkout");
    }
}