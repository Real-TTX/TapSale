using TapSale.Web.Models;
using TapSale.Web.Services;

namespace TapSale.Tests;

public sealed class SaleCalculatorTests
{
    [Theory]
    [InlineData(ProductKind.Product, 350, 2, 700)]
    [InlineData(ProductKind.DepositCharge, 200, 3, 600)]
    [InlineData(ProductKind.DepositReturn, 200, 3, -600)]
    public void LineTotal_UsesProductDirection(ProductKind kind, long price, int quantity, long expected)
        => Assert.Equal(expected, SaleCalculator.LineTotal(kind, price, quantity));

    [Fact]
    public void Change_ReturnsDifference() => Assert.Equal(650, SaleCalculator.Change(1350, 2000));

    [Fact]
    public void Change_RejectsInsufficientCash() => Assert.Throws<InvalidOperationException>(() => SaleCalculator.Change(1350, 1000));

    [Fact]
    public void Payout_DoesNotCreateChange() => Assert.Equal(0, SaleCalculator.Change(-400, 0));
}
