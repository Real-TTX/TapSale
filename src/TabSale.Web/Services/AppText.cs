using System.Globalization;

namespace TabSale.Web.Services;

public sealed class AppText
{
    private static readonly Dictionary<string, (string De, string En)> Values = new()
    {
        ["Sell"] = ("Verkaufen", "Sell"), ["History"] = ("Historie", "History"),
        ["Users"] = ("Benutzer", "Users"), ["SaleLists"] = ("Verkaufslisten", "Sale lists"),
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
        ["Ready"] = ("Bereit", "Ready"), ["Pending"] = ("ausstehend", "pending"), ["Synced"] = ("Synchronisiert", "Synced"), ["NoList"] = ("Keine Verkaufsliste zugewiesen", "No sale list assigned"), ["NoListHelp"] = ("Bitte einen Admin um die Zuweisung einer Verkaufsliste.", "Ask an administrator to assign a sale list."),
        ["Given"] = ("Gegeben", "Given"), ["ConfirmPayout"] = ("Bestätige die Barauszahlung an den Kunden.", "Confirm the cash payout to the customer."), ["Close"] = ("Schließen", "Close"),
        ["SetupHelp"] = ("Erstelle den ersten Administrator. Danach ist diese Seite dauerhaft deaktiviert.", "Create the first administrator. This page is permanently disabled afterwards."),
        ["LoginHelp"] = ("Schnell und zuverlässig verkaufen – auch offline.", "Fast, reliable selling — even offline."), ["DeleteConfirm"] = ("Wirklich endgültig löschen?", "Delete permanently?")
    };

    public string this[string key] => Values.TryGetValue(key, out var value)
        ? (CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "de" ? value.De : value.En)
        : key;
}
