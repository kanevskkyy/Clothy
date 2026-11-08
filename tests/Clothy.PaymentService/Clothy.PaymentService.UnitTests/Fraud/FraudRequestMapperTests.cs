using Clothy.PaymentService.BLL.Fraud;
using FluentAssertions;
using Xunit;

namespace Clothy.PaymentService.UnitTests.Fraud;

public class FraudRequestMapperTests
{
    [Fact]
    public void CardFingerprintToCard1_IsStableExactIntegerBelow2Pow53()
    {
        double a = FraudRequestMapper.CardFingerprintToCard1("Xt5EWLLDS7FJjR1c");
        double b = FraudRequestMapper.CardFingerprintToCard1("Xt5EWLLDS7FJjR1c");

        a.Should().Be(b);
        a.Should().BeGreaterThanOrEqualTo(0).And.BeLessThan(Math.Pow(2, 53));
        Math.Floor(a).Should().Be(a);
        // survives a round trip through decimal text (JSON) without rounding
        double.Parse(a.ToString("R")).Should().Be(a);
    }

    [Fact]
    public void CardFingerprintToCard1_DiffersForDifferentCards()
    {
        FraudRequestMapper.CardFingerprintToCard1("card-a")
            .Should().NotBe(FraudRequestMapper.CardFingerprintToCard1("card-b"));
    }

    [Theory]
    [InlineData("visa", "visa")]
    [InlineData("Mastercard", "mastercard")]
    [InlineData("amex", "american express")]
    [InlineData("discover", "discover")]
    [InlineData("unknown", null)]
    [InlineData(null, null)]
    public void MapBrand_UsesIeeeCisValues(string? brand, string? expected)
    {
        FraudRequestMapper.MapBrand(brand).Should().Be(expected);
    }

    [Theory]
    [InlineData("credit", "credit")]
    [InlineData("debit", "debit")]
    [InlineData("prepaid", null)]
    [InlineData("unknown", null)]
    public void MapFunding_KeepsOnlyCreditAndDebit(string funding, string? expected)
    {
        FraudRequestMapper.MapFunding(funding).Should().Be(expected);
    }

    [Theory]
    [InlineData("01001", 1001d)]
    [InlineData(" 79000 ", 79000d)]
    [InlineData("SW1A 1AA", null)]
    [InlineData("12345-6789", null)]
    [InlineData("", null)]
    [InlineData(null, null)]
    public void PostalCodeToAddr1_IsNumberOnlyForDigits(string? postal, double? expected)
    {
        FraudRequestMapper.PostalCodeToAddr1(postal).Should().Be(expected);
    }

    [Theory]
    [InlineData("Some.One@Gmail.com", "gmail.com")]
    [InlineData("a@ukr.net", "ukr.net")]
    [InlineData("no-at-sign", null)]
    [InlineData("trailing@", null)]
    [InlineData(null, null)]
    public void EmailDomain_TakesLowercaseDomain(string? email, string? expected)
    {
        FraudRequestMapper.EmailDomain(email).Should().Be(expected);
    }

    [Fact]
    public void Build_FillsDeviceFieldsAndLeavesUnknownOnesNull()
    {
        FraudScoreRequest request = FraudRequestMapper.Build(new FraudPaymentData(
            Guid.NewGuid(), DateTime.UtcNow, 10m, "fp", null, null, null, null,
            new FraudDeviceData("mobile", "iOS Device", "iOS 17.1.2", "mobile safari 17.1", "2556x1179")));

        request.ProductCD.Should().Be("W");
        request.DeviceType.Should().Be("mobile");
        request.DeviceInfo.Should().Be("iOS Device");
        request.Id30.Should().Be("iOS 17.1.2");
        request.Id31.Should().Be("mobile safari 17.1");
        request.Id33.Should().Be("2556x1179");
        request.Card4.Should().BeNull();
        request.Addr1.Should().BeNull();
        request.PEmailDomain.Should().BeNull();
    }

    [Fact]
    public void DeviceMetadata_RoundTripsThroughStripeMetadata()
    {
        Dictionary<string, string> metadata = new();
        FraudDeviceData device = new FraudDeviceData("desktop", "Windows", "Windows 10", "chrome 120.0", "1920x1080");

        FraudDeviceMetadata.AddTo(metadata, device);

        FraudDeviceMetadata.From(metadata).Should().Be(device);
        FraudDeviceMetadata.From(new Dictionary<string, string>()).Should().BeNull();
    }
}
