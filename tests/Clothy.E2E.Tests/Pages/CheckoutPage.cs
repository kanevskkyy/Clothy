using OpenQA.Selenium;

namespace Clothy.E2E.Tests.Pages;

public class CheckoutPage
{
    private readonly SeleniumBase ctx;

    public CheckoutPage(SeleniumBase ctx) => this.ctx = ctx;

    public void GoToCheckout() => ctx.GoTo("/checkout");

    public void FillPersonalInfo(string firstName, string lastName, string email, string phone, string comment = "")
    {
        IWebElement firstNameEl = ctx.ById("firstName");
        firstNameEl.Clear();
        firstNameEl.SendKeys(firstName);

        IWebElement lastNameEl = ctx.ById("lastName");
        lastNameEl.Clear();
        lastNameEl.SendKeys(lastName);

        IWebElement emailEl = ctx.ById("email");
        emailEl.Clear();
        emailEl.SendKeys(email);

        IWebElement phoneEl = ctx.ById("phoneNumber");
        phoneEl.Clear();
        phoneEl.SendKeys(phone);

        if (!string.IsNullOrEmpty(comment))
        {
            IWebElement commentEl = ctx.ById("comment");
            commentEl.Clear();
            commentEl.SendKeys(comment);
        }
    }

    public void SelectPaymentCard() => ctx.Driver.FindElement(By.Id("payment-card")).Click();

    public void SelectPaymentCrypto() => ctx.Driver.FindElement(By.Id("payment-crypto")).Click();

    public void SelectDeliveryProvider(string providerId) => ctx.Driver.FindElement(By.Id($"delivery-{providerId}")).Click();

    public void ClickPlaceOrder() => ctx.Driver.FindElement(By.XPath("//button[contains(text(),'Create an order')]")).Click();

    public bool IsOnCheckoutPage() => ctx.Driver.Url.Contains("/checkout");

    public string GetOrderWarningText() => ctx.Driver.FindElement(By.XPath("//*[contains(text(),'10 minutes')]")).Text;
}