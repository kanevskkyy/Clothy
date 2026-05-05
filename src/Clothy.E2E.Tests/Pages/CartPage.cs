using OpenQA.Selenium;

namespace Clothy.E2E.Tests.Pages;

public class CartPage
{
    private readonly SeleniumBase ctx;

    public CartPage(SeleniumBase ctx) => this.ctx = ctx;

    public void GoToCart() => ctx.GoTo("/cart");

    public int GetCartItemCount() => ctx.AllByCss("div[class*='basketCard']").Count;

    public bool IsCartEmpty() => ctx.ElementExists(By.XPath("//*[contains(text(),'Your cart is empty')]"));

    public void ClickIncrease(int itemIndex = 0)
    {
        IReadOnlyCollection<IWebElement> cards = ctx.AllByCss("div[class*='basketCard']");
        cards.ElementAt(itemIndex).FindElement(By.CssSelector("button[class*='plus']")).Click();
    }

    public void ClickDecrease(int itemIndex = 0)
    {
        IReadOnlyCollection<IWebElement> cards = ctx.AllByCss("div[class*='basketCard']");
        cards.ElementAt(itemIndex).FindElement(By.CssSelector("button[class*='minus']")).Click();
    }

    public void RemoveItem(int itemIndex = 0)
    {
        IReadOnlyCollection<IWebElement> cards = ctx.AllByCss("div[class*='basketCard']");
        cards.ElementAt(itemIndex).FindElement(By.CssSelector("button[class*='close']")).Click();
    }

    public void ClickClearAll() =>
        ctx.Driver.FindElement(By.XPath("//button/span[text()='Clear All']")).Click();

    public void ClickCheckout() =>
        ctx.Driver.FindElement(By.XPath("//button/span[normalize-space()='Checkout →']")).Click();

    public string GetTotalPrice() => ctx.Driver.FindElement(By.XPath("//*[contains(@class,'totalRow')]//span[last()]")).Text;

    public string GetQuantity(int itemIndex = 0)
    {
        IReadOnlyCollection<IWebElement> cards = ctx.AllByCss("div[class*='basketCard']");
        return cards.ElementAt(itemIndex).FindElement(By.CssSelector("div[class*='quantity']")).Text;
    }
}