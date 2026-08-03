using System.Globalization;

namespace TabSale.Web.Services;

public sealed class AppText
{
    private static readonly Dictionary<string, (string De, string En)> Values = new()
    {
        ["Sell"] = ("Verkaufen", "Sell"), ["History"] = ("Historie", "History"),
        ["Users"] = ("Benutzer", "Users"), ["SaleLists"] = ("Verkaufslisten", "Sale lists"),
        ["ChooseSaleList"] = ("Verkaufsliste auswählen", "Choose sale list"),
        ["Login"] = ("Anmelden", "Sign in"), ["Logout"] = ("Abmelden", "Sign out"),
        ["Save"] = ("Speichern", "Save"), ["Back"] = ("Zurück", "Back"),
        ["Delete"] = ("Löschen", "Delete"), ["Search"] = ("Suchen", "Search"),
        ["Total"] = ("Gesamtsumme", "Total"), ["Pay"] = ("Bezahlen", "Pay"),
        ["Payout"] = ("Auszahlen", "Pay out"), ["Cash"] = ("Bargeld", "Cash"),
        ["Change"] = ("Rückgeld", "Change"), ["Complete"] = ("Abschließen", "Complete"),
        ["Offline"] = ("Offline", "Offline"), ["Sync"] = ("Synchronisieren", "Sync"),
        ["TileMode"] = ("Kacheln", "Tiles"), ["ListMode"] = ("Liste", "List"),
        ["OwnSales"] = ("Meine Verkäufe", "My sales"), ["Setup"] = ("Ersteinrichtung", "Initial setup"),
        ["UserName"] = ("Benutzername", "Username"), ["Password"] = ("Passwort", "Password"),
        ["DisplayName"] = ("Anzeigename", "Display name"), ["Language"] = ("Sprache", "Language"),
        ["Administration"] = ("Verwaltung", "Administration"), ["Reports"] = ("Auswertungen", "Reports"),
        ["Active"] = ("Aktiv", "Active"), ["Archived"] = ("Archiviert", "Archived"), ["Status"] = ("Status", "Status"),
        ["AllStatus"] = ("Alle Status", "All status"), ["Name"] = ("Name", "Name"), ["Products"] = ("Produkte", "Products"),
        ["Product"] = ("Produkt", "Product"), ["NewProduct"] = ("Neues Produkt", "New product"), ["NewSaleList"] = ("Neue Verkaufsliste", "New sale list"),
        ["Edit"] = ("Bearbeiten", "Edit"), ["Price"] = ("Preis", "Price"), ["EndPrice"] = ("Endpreis (€)", "End price (€)"), ["Type"] = ("Typ", "Type"),
        ["NewUser"] = ("Neuer Benutzer", "New user"), ["Role"] = ("Rolle", "Role"), ["AllRoles"] = ("Alle Rollen", "All roles"),
        ["AssignedLists"] = ("Zugewiesene Verkaufslisten", "Assigned sale lists"), ["NewPassword"] = ("Neues Passwort (optional)", "New password (optional)"),
        ["DeviceSessions"] = ("Aktive Geräte-Sessions", "Active device sessions"), ["Device"] = ("Gerät", "Device"), ["LastActive"] = ("Zuletzt aktiv", "Last active"), ["LastSync"] = ("Letzte Synchronisierung", "Last sync"), ["Revoke"] = ("Widerrufen", "Revoke"),
        ["MySales"] = ("Meine Verkäufe", "My sales"), ["Result"] = ("Ergebnis", "Result"), ["Transactions"] = ("Vorgänge", "Transactions"), ["AllSaleLists"] = ("Alle Verkaufslisten", "All sale lists"),
        ["Filter"] = ("Filtern", "Filter"), ["Time"] = ("Zeit", "Time"), ["Receipt"] = ("Beleg", "Receipt"), ["Details"] = ("Details", "Details"), ["ExportCsv"] = ("CSV exportieren", "Export CSV"), ["Share"] = ("Teilen", "Share"),
        ["Sold"] = ("Verkauft", "Sold"), ["Quantity"] = ("Menge", "Quantity"), ["CancelSale"] = ("Verkauf stornieren", "Cancel sale"),
        ["CashShift"] = ("Kassenschicht", "Cash shift"), ["Optional"] = ("Optional", "Optional"), ["NoActiveShift"] = ("Keine aktive Schicht", "No active shift"),
        ["NoShiftHelp"] = ("Du kannst ohne Schicht verkaufen oder eine Schicht mit Anfangsbestand starten.", "You can keep selling without a shift or start one with an opening cash balance."),
        ["OpeningBalance"] = ("Anfangsbestand (€)", "Opening balance (€)"), ["StartShift"] = ("Schicht starten", "Start shift"), ["ExpectedCash"] = ("Erwarteter Bargeldbestand", "Expected cash"), ["CountedCash"] = ("Gezähltes Bargeld (€)", "Counted cash (€)"), ["CloseShift"] = ("Schicht abschließen", "Close shift"),
        ["ShiftName"] = ("Schichtname", "Shift name"), ["ShiftNamePlaceholder"] = ("z. B. Samstag Frühschicht", "e.g. Saturday morning shift"),
        ["Shifts"] = ("Schichten", "Shifts"), ["Opened"] = ("Beginn", "Opened"), ["Closed"] = ("Ende", "Closed"), ["StillOpen"] = ("Läuft", "Open"),
        ["SoldItems"] = ("Verkaufte Artikel", "Items sold"), ["CashDifference"] = ("Kassendifferenz", "Cash difference"), ["UnnamedShift"] = ("Ohne Namen", "Unnamed shift"),
        ["Ready"] = ("Bereit", "Ready"), ["Pending"] = ("ausstehend", "pending"), ["Synced"] = ("Synchronisiert", "Synced"), ["NoList"] = ("Keine Verkaufsliste zugewiesen", "No sale list assigned"), ["NoListHelp"] = ("Bitte einen Admin um die Zuweisung einer Verkaufsliste.", "Ask an administrator to assign a sale list."),
        ["Given"] = ("Gegeben", "Given"), ["ConfirmPayout"] = ("Bestätige die Barauszahlung an den Kunden.", "Confirm the cash payout to the customer."), ["Close"] = ("Schließen", "Close"),
        ["SetupHelp"] = ("Erstelle den ersten Administrator. Danach ist diese Seite dauerhaft deaktiviert.", "Create the first administrator. This page is permanently disabled afterwards."),
        ["LoginHelp"] = ("Schnell und zuverlässig verkaufen – auch offline.", "Fast, reliable selling — even offline."), ["DeleteConfirm"] = ("Wirklich endgültig löschen?", "Delete permanently?"),
        ["CustomTheme"] = ("Eigenes CI", "Custom brand"), ["Colors"] = ("Farben", "Colors"), ["NavigationColor"] = ("Navigation", "Navigation"),
        ["PrimaryColor"] = ("Primär", "Primary"), ["AccentColor"] = ("Akzent", "Accent"), ["BackgroundColor"] = ("Hintergrund", "Background"),
        ["DangerColor"] = ("Warnung", "Danger"), ["ResetColors"] = ("Farben zurücksetzen", "Reset colors"),
        ["Appearance"] = ("Darstellung", "Appearance"), ["AppearanceSaved"] = ("Darstellung wurde für alle Benutzer gespeichert.", "Appearance was saved for all users."),
        ["CustomColorsHelp"] = ("Die Farben werden verwendet, wenn „Eigenes CI“ gewählt ist.", "The colors are used when “Custom brand” is selected."),
        ["ChooseTheme"] = ("Theme auswählen", "Choose theme"), ["WinterTheme"] = ("Winter", "Winter"), ["MarketTheme"] = ("Markt", "Market"),
        ["ContrastTheme"] = ("Hoher Kontrast", "High contrast"), ["CustomColors"] = ("Eigene CI-Farben", "Custom brand colors"),
        ["ThemePreview"] = ("Live-Vorschau", "Live preview"), ["PreviewHeadline"] = ("So wirkt dein Theme", "How your theme looks"), ["PreviewProduct"] = ("Beispielprodukt", "Sample product"),
        ["InvalidHexColor"] = ("Bitte das Format #RRGGBB verwenden.", "Use the format #RRGGBB."),
        ["Categories"] = ("Kategorien", "Categories"), ["Category"] = ("Kategorie", "Category"), ["NewCategory"] = ("Neue Kategorie", "New category"),
        ["CategoryDisplay"] = ("Kategorien im Verkauf", "Categories while selling"), ["CategoryFilterMode"] = ("Als Filter", "As filters"),
        ["CategorySectionsMode"] = ("Als Abschnitte", "As sections"), ["CategoryDrilldownMode"] = ("Zweistufig mit Zurück", "Two levels with back"),
        ["BackToCategories"] = ("Zurück zu den Kategorien", "Back to categories"),
        ["ProductList"] = ("Produktliste", "Product list"), ["SaleListContent"] = ("Inhalt der Verkaufsliste", "Sale list content"),
        ["NoCategory"] = ("Ohne Kategorie", "No category"), ["AllCategories"] = ("Alle", "All"), ["OtherCategory"] = ("Sonstiges", "Other"),
        ["ProductImage"] = ("Produktbild", "Product image"), ["RemoveImage"] = ("Vorhandenes Bild entfernen", "Remove current image"),
        ["ImageHelp"] = ("PNG, JPG, GIF oder WebP, maximal 5 MB. Ohne Bild wird das gewählte Icon angezeigt.", "PNG, JPG, GIF or WebP, maximum 5 MB. The selected icon is shown when no image is stored."),
        ["FallbackIcon"] = ("Icon ohne Bild", "Icon without image"), ["Icon"] = ("Icon", "Icon"), ["Color"] = ("Farbe", "Color"), ["SortOrder"] = ("Sortierung", "Sort order"),
        ["InvalidImage"] = ("Bitte ein gültiges Bild bis maximal 5 MB auswählen.", "Choose a valid image up to 5 MB."),
        ["InvalidIcon"] = ("Bitte ein verfügbares Icon auswählen.", "Choose an available icon."), ["InvalidCategory"] = ("Die gewählte Kategorie gehört nicht zu dieser Verkaufsliste.", "The selected category does not belong to this sale list."),
        ["CategoryExists"] = ("Eine Kategorie mit diesem Namen existiert bereits.", "A category with this name already exists."),
        ["IconTag"] = ("Allgemein", "General"), ["IconDrink"] = ("Getränk", "Drink"), ["IconBeer"] = ("Bier", "Beer"), ["IconFood"] = ("Essen", "Food"),
        ["IconSausage"] = ("Wurst", "Sausage"), ["IconDessert"] = ("Nachtisch", "Dessert"), ["IconCoffee"] = ("Kaffee", "Coffee"), ["IconWine"] = ("Wein", "Wine"),
        ["IconSnack"] = ("Snack", "Snack"), ["IconIceCream"] = ("Eis", "Ice cream"), ["IconTicket"] = ("Ticket", "Ticket"), ["IconDeposit"] = ("Pfand", "Deposit"),
        ["RestaurantMode"] = ("Kassenlayout", "POS layout"), ["RestaurantModeHelp"] = ("Restaurant-Kassenlayout ein- oder ausschalten", "Enable or disable the restaurant POS layout"),
        ["Order"] = ("Stückliste", "Order"), ["CurrentOrder"] = ("Aktueller Bon", "Current ticket"), ["EmptyOrder"] = ("Noch keine Produkte ausgewählt.", "No products selected yet."),
        ["Item"] = ("Artikel", "item"), ["Items"] = ("Artikel", "items")
    };

    public string this[string key] => Values.TryGetValue(key, out var value)
        ? (CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "de" ? value.De : value.En)
        : key;
}
