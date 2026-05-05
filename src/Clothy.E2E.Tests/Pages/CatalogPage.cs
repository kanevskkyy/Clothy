using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace Clothy.E2E.Tests.Pages;

public class CatalogPage
{
    private readonly SeleniumBase ctx;

    public CatalogPage(SeleniumBase ctx) => this.ctx = ctx;

    public void GoToCatalog() => ctx.GoTo("/catalog");

    public void GoToClotheBySlug(string slug, string colorSlug) => ctx.GoTo($"/clothe/{slug}/{colorSlug}");

    public int GetProductCardCount() => ctx.AllByCss("a[class*='card']").Count;

    public void OpenFirstProduct()
    {
        ctx.WaitForElement(By.CssSelector("a[class*='card']"));
        Thread.Sleep(500);
        ctx.AllByCss("a[class*='card']").First().Click();
        ctx.WaitForElement(By.TagName("h1"));
    }

    public void SelectSortOption(string value)
    {
        IWebElement select = ctx.Driver.FindElement(By.TagName("select"));
        SelectElement selectEl = new SelectElement(select);
        selectEl.SelectByValue(value);
    }

    public bool HasProductCards() => ctx.AllByCss("a[class*='card']").Count > 0;

    public bool HasEmptyState() => ctx.ElementExists(By.XPath("//*[contains(text(),'No items found')]"));

    public void SelectSize(string sizeName)
    {
        IReadOnlyCollection<IWebElement> sizes = ctx.AllByCss("div[class*='size']");
        IWebElement? target = sizes.FirstOrDefault(s =>
            s.Text.Trim() == sizeName && !s.GetAttribute("class").Contains("sizeNotAvailable"));
        target?.Click();
    }

    public void SelectColorByIndex(int index)
    {
        IReadOnlyCollection<IWebElement> colors = ctx.AllByCss("div[class*='color']");
        if (index < colors.Count) colors.ElementAt(index).Click();
    }

    public void ClickAddToCart() => ctx.Driver.FindElement(By.XPath("//button/span[text()='Add to cart']")).Click();


    public void ClickSubscribeToUpdates() => ctx.Driver.FindElement(By.XPath("//button[contains(text(),'Subscribe to updates')]")).Click();

    public bool HasAddToCartButton()
    {
        ctx.WaitForElement(By.TagName("h1"));
        Thread.Sleep(1000);
        return ctx.ElementExists(By.XPath("//button/span[text()='Add to cart']"));
    }

    public bool HasSubscribeButton()
    {
        return ctx.ElementExists(By.XPath("//button/span[text()='Subscribe to updates']"));
    }

    public string GetProductName() => ctx.Driver.FindElement(By.CssSelector("h1[class*='clotheName']")).Text;

    public void ClickTryOn() => ctx.Driver.FindElement(By.XPath("//button[contains(text(),'Try it on')]")).Click();
}