namespace TabSale.Web.Services;

public static class ProductIconCatalog
{
    public sealed record Option(string Key, string Symbol, string TextKey);

    public static IReadOnlyList<Option> All { get; } =
    [
        new("tag", "🏷️", "IconTag"),
        new("drink", "🥤", "IconDrink"),
        new("beer", "🍺", "IconBeer"),
        new("food", "🍽️", "IconFood"),
        new("sausage", "🌭", "IconSausage"),
        new("dessert", "🍰", "IconDessert"),
        new("coffee", "☕", "IconCoffee"),
        new("wine", "🍷", "IconWine"),
        new("snack", "🥨", "IconSnack"),
        new("icecream", "🍦", "IconIceCream"),
        new("ticket", "🎟️", "IconTicket"),
        new("deposit", "↩️", "IconDeposit")
    ];

    public static bool Contains(string? key) => All.Any(x => x.Key == key);
}
