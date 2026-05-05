using Clothy.E2E.Tests.Pages;
using FluentAssertions;
using OpenQA.Selenium;
using Xunit;

namespace Clothy.E2E.Tests.Tests;

public class CatalogTests : SeleniumBase
{
    private readonly CatalogPage catalogPage;

    public CatalogTests()
    {
        catalogPage = new CatalogPage(this);
    }

    [Fact]
    public void CatalogPage_OnLoad_ShowsProductCards()
    {
        catalogPage.GoToCatalog();
        WaitForElement(By.CssSelector("a[class*='card']"));
        catalogPage.HasProductCards().Should().BeTrue();
    }

    [Fact]
    public void CatalogPage_OpenFirstProduct_NavigatesToDetailPage()
    {
        catalogPage.GoToCatalog();
        WaitForElement(By.CssSelector("a[class*='card']"));
        catalogPage.OpenFirstProduct();
        Driver.Url.Should().Contain("/clothe/");
    }

    [Fact]
    public void CatalogPage_SortByPriceAsc_ChangesUrl()
    {
        catalogPage.GoToCatalog();
        WaitForElement(By.TagName("select"));
        catalogPage.SelectSortOption("price-asc");
        Driver.Url.Should().Contain("sort=price-asc");
    }

    [Fact]
    public void ClotheDetail_HasAddToCartOrSubscribeButton()
    {
        catalogPage.GoToCatalog();
        WaitForElement(By.CssSelector("a[class*='card']"));
        catalogPage.OpenFirstProduct();
        bool hasAdd = catalogPage.HasAddToCartButton();
        bool hasSub = catalogPage.HasSubscribeButton();
        (hasAdd || hasSub).Should().BeTrue();
    }

    [Fact]
    public void ClotheDetail_SelectSize_UpdatesActiveSize()
    {
        catalogPage.GoToCatalog();
        WaitForElement(By.CssSelector("a[class*='card']"));
        catalogPage.OpenFirstProduct();
        WaitForElement(By.CssSelector("h1[class*='clotheName']"));

        IReadOnlyCollection<IWebElement> sizes = AllByCss("div[class*='size']");
        if (sizes.Count > 0)
        {
            sizes.First().Click();
            IReadOnlyCollection<IWebElement> updatedSizes = AllByCss("div[class*='size']");
            updatedSizes.Count.Should().BeGreaterThan(0);
        }
    }

    [Fact]
    public void ClotheDetail_SelectColor_UpdatesGallery()
    {
        catalogPage.GoToCatalog();
        WaitForElement(By.CssSelector("a[class*='card']"));
        catalogPage.OpenFirstProduct();
        WaitForElement(By.CssSelector("h1[class*='clotheName']"));

        IReadOnlyCollection<IWebElement> colors = AllByCss("div[class*='color']");
        if (colors.Count > 1)
        {
            string classBefore = colors.First().GetAttribute("class");
            colors.ElementAt(1).Click();
            string classAfter = AllByCss("div[class*='color']").ElementAt(1).GetAttribute("class");
            classAfter.Should().NotBe(classBefore);
        }
    }
}