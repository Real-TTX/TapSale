using TabSale.Web.Models;

namespace TabSale.Web.Services;

public static class SaleCalculator
{
    public static long LineTotal(ProductKind kind, long unitPriceCents, int quantity)
    {
        if (unitPriceCents < 0) throw new ArgumentOutOfRangeException(nameof(unitPriceCents));
        if (quantity <= 0 || quantity > 999) throw new ArgumentOutOfRangeException(nameof(quantity));
        return checked((kind == ProductKind.DepositReturn ? -1L : 1L) * unitPriceCents * quantity);
    }

    public static long Change(long totalCents, long tenderedCents)
    {
        if (totalCents <= 0) return 0;
        if (tenderedCents < totalCents) throw new InvalidOperationException("Tendered cash is insufficient.");
        return tenderedCents - totalCents;
    }
}
