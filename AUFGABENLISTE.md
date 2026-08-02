# TabSale – Aufgabenliste

Stand: 02.08.2026

## 0. Offene Entscheidungen vor der Implementierung

- [x] Frontend-Grundlage festgelegt: ASP.NET Core mit Razor.
- [x] Razor-Ausführungsmodell festgelegt: klassische Razor Pages; die offlinefähige Verkaufsoberfläche wird clientseitig mit JavaScript, Service Worker und IndexedDB ergänzt.
- [ ] Offline-Datenfluss festlegen: Verkäufe zuerst lokal im Browser speichern und bei erreichbarem Server synchronisieren.
- [x] Mehrgerätebetrieb festgelegt: Dieselbe Verkaufsliste darf gleichzeitig auf mehreren Smartphones genutzt werden; Offline-Verkäufe werden später ohne Überschreiben zusammengeführt.
- [x] Anmeldeverhalten festgelegt: Die erstmalige Anmeldung benötigt den erreichbaren TabSale-Server; danach darf der Benutzer auf dem bereits autorisierten Gerät offline weiterarbeiten.
- [x] Position der Gesamtsumme festgelegt: oben dauerhaft als großes Taschenrechner-Display und zusätzlich im unten fixierten, einhändig erreichbaren Bezahl-Balken.
- [ ] Rollen und Berechtigungen finalisieren: `Admin`, `Verwalter`, `User`.
- [x] Preisumfang festgelegt: zunächst nur ein Endpreis; Datenmodell und Export für spätere Mehrwertsteuer-, Netto- und Steuerangaben erweiterbar halten.
- [x] Korrekturprinzip festgelegt: Abgeschlossene Verkäufe bleiben unveränderbar; Fehler werden ausschließlich durch eine verknüpfte, auditierbare Storno-Gegenbuchung korrigiert.
- [x] Pfandprinzip festgelegt: Pfandberechnung und Pfandrückgabe sind getrennte Positionstypen; die Rückgabe reduziert den Endbetrag, ohne negative Stammdatenpreise zu verwenden.
- [x] Reine Pfandrückgaben dürfen einen negativen Gesamtbetrag erzeugen und werden als Barauszahlung/Kassenabgang abgeschlossen.
- [x] Backup-Umfang festgelegt: keine anwendungsinternen Backups; gesichert wird das vollständige persistente Docker-Volume.
- [x] Sprachumfang festgelegt: Deutsch und Englisch; Währung bleibt Euro.

## 1. Repository und Projektgrundlage

- [ ] Git-Branches `main` (Release) und `dev` (Entwicklung) anlegen.
- [ ] Git-Autorname und E-Mail des Auftraggebers vor dem ersten Commit prüfen/konfigurieren.
- [ ] Solution- und Projektstruktur für Frontend, API, Domain, Persistenz und Tests anlegen.
- [ ] `.gitignore`, `.editorconfig` und grundlegende Projektkonventionen ergänzen.
- [ ] README mit lokaler Einrichtung, Docker-Befehlen und Architekturüberblick erstellen.
- [ ] Zentrale Build- und Versionsinformationen einrichten.
- [ ] Versionsschema automatisieren:
  - Release: `<major>.<minor>.<build>-<builddate>`
  - Development: `nightly-<build>-<builddate>`
  - Local: `local-<builddate>`

## 2. Architektur und Datenmodell

- [ ] Fachmodelle entwerfen: Benutzer, Rolle, Verkaufslisten, Produkte, Positionstypen inklusive Pfand, Zuweisungen, Verkauf, Verkaufsposition, Stornoverknüpfung und Session.
- [x] Schichtprinzip festgelegt: optionale Kassenschichten mit Anfangsbestand, Soll-Endbestand, gezähltem Endbestand und Kassendifferenz; Verkauf bleibt auch ohne Schicht möglich.
- [x] Schichtkonfiguration festgelegt: Schichten sind systemweit optional und nicht pro Verkaufsliste konfigurierbar.
- [ ] Fachmodell für optionale Kassenschichten und deren Benutzer-/Verkaufslistenbezug ergänzen.
- [x] Zahlungsumfang festgelegt: im ersten Release ausschließlich Barzahlungen und Barauszahlungen; Zahlungsart im Datenmodell für spätere Erweiterungen separat speichern.
- [ ] SQLite als persistente serverseitige Datenbank einrichten.
- [ ] Tabellennamen in PascalCase konfigurieren.
- [ ] Für persistente Datensätze die Auditfelder `CreateDate`, `CreateUserId`, `UpdateDate`, `UpdateUserId` vorsehen.
- [ ] Primärschlüssel einheitlich als `Id` mit 64-Bit-Integer verwenden.
- [ ] Sicherheitsschlüssel getrennt als Token/UUID modellieren, z. B. bei `UserSession`.
- [ ] Migrationen und automatische Initialisierung der Datenbank einrichten.
- [ ] JSON-Konfiguration für nichtfachliche Einstellungen definieren.
- [x] Erstadministration festgelegt: Beim ersten Start erscheint ein einmaliger Einrichtungsassistent für Benutzername, sicheres Passwort und Sprache; danach ist er dauerhaft gesperrt.
- [ ] Serverseitig abgesicherten Bootstrap-Status in der Datenbank speichern und den Assistenten nach Anlage des ersten Admins vollständig deaktivieren.
- [ ] Optionale Demo-Daten ausschließlich explizit für Development-Builds vorsehen.
- [ ] Schichten und Business-Services übersichtlich halten; unnötig tiefe Verschachtelung vermeiden.

## 3. Docker und Laufzeit

- [ ] Mehrstufiges `Dockerfile` auf Basis der passenden ASP.NET-Core-Images erstellen.
- [ ] Development-Compose-Konfiguration mit Port `9262` erstellen.
- [ ] Release-Compose-Konfiguration mit Port `9262` erstellen.
- [ ] Persistentes Volume für SQLite, Konfiguration und notwendige Schlüssel einrichten.
- [ ] Alle wiederherstellungsrelevanten Daten in einem klar dokumentierten persistenten Volume bündeln.
- [ ] Persistenz von Benutzer-Sessions über Container-Neustarts sicherstellen.
- [ ] Healthcheck und geordnetes Startverhalten ergänzen.
- [ ] Produktionsnahe Konfiguration ohne eingecheckte Geheimnisse vorbereiten.
- [ ] Standard-Testablauf dokumentieren: Container neu bauen und Stack neu deployen.

## 4. PWA- und Offline-Grundlage

- [ ] Web-App-Manifest, Icons, Theme-Farben und installierbare PWA konfigurieren.
- [ ] Service Worker und App-Shell-Caching für vollständigen Offline-Start umsetzen.
- [ ] Lokale Browser-Datenbank, vorzugsweise IndexedDB, abstrahieren.
- [ ] Aktiven Warenkorb und noch nicht synchronisierte Verkäufe lokal speichern.
- [ ] Offline-Queue mit eindeutiger Geräte-ID, stabilen Verkaufs-IDs und idempotenter Synchronisation entwickeln.
- [ ] Produkt- und Preisstände versionieren, damit Offline-Verkäufe stets mit dem beim Verkauf gültigen Preis gespeichert werden.
- [ ] Synchronisationsstatus und letzte erfolgreiche Synchronisation sichtbar machen.
- [ ] Konflikt- und Fehlerstrategie für geänderte Produkte/Preise definieren.
- [ ] Verhalten bei Speicherlimit, privatem Browsermodus und gelöschten Browserdaten behandeln.
- [ ] Offline-Funktion im echten Flugmodus testen.

## 5. Anmeldung, Sessions und Rechte

- [ ] Lokale Benutzeranmeldung mit sicherem Passwort-Hashing implementieren.
- [ ] Persistente, widerrufbare Sessions mit zufälligem Token entwickeln.
- [x] Session-Laufzeit festgelegt: gerätegebundene Anmeldung ohne feste Ablaufzeit bis zur Abmeldung oder zum Widerruf durch einen Admin.
- [x] Anmeldeablauf festgelegt: Benutzername und Passwort nur bei der tatsächlichen Anmeldung; keine zusätzliche PIN oder wiederkehrende Entsperrung während einer aktiven Session.
- [ ] Session-Widerrufe bei der nächsten Verbindung auf Offline-Geräte übertragen; Container-Neustarts dürfen Sessions nicht beenden.
- [x] Mehrfachanmeldung festgelegt: Derselbe Benutzer darf gleichzeitig auf mehreren Geräten angemeldet sein.
- [ ] Geräte-Sessions im Adminbereich mit Gerätename, letzter Aktivität und letzter Synchronisation einzeln anzeigen und widerrufbar machen.
- [ ] Erneute Passwortbestätigung für besonders sensible Verwaltungsaktionen vorsehen.
- [ ] Sichere Cookie-/Token-Einstellungen und Schutz vor CSRF/XSS berücksichtigen.
- [ ] Rollenbasierte Autorisierung zentral umsetzen.
- [ ] Rechte-Matrix definieren und automatisiert testen:
  - Admin: Benutzer, Rollen und Verkaufslisten-Zuweisungen verwalten;
  - Verwalter: zugewiesene Verkaufslisten und Produkte bearbeiten sowie deren vollständige Historien, Kassenstürze und Exporte einsehen;
  - User: zugewiesene Verkaufslisten verwenden und ausschließlich die eigenen Verkäufe sowie die persönliche Schichtsumme sehen.
- [ ] Gerätegebundene Offline-Anmeldung nach einer erfolgreichen Online-Erstanmeldung umsetzen; Sperren und Rollenänderungen bei der nächsten Serververbindung übernehmen.
- [ ] Abmelden, Session-Ablauf und Session-Widerruf implementieren.

## 6. UI-Grundsystem

- [ ] Mobile-first Layout mit linker Navigation für `Verkaufen` und `Historie` erstellen.
- [ ] Navigation auf kleinen Displays als platzsparendes Drawer-/Overlay-Menü ausführen.
- [ ] Vollständige Lokalisierungsbasis mit ASP.NET-Core-Ressourcen für Deutsch und Englisch einrichten.
- [ ] Sprachumschalter integrieren und die Auswahl je Benutzer sowie offline auf dem Gerät speichern.
- [ ] Beim Erstaufruf die Browsersprache erkennen; Deutsch und Englisch übernehmen, alle anderen Sprachen auf Englisch zurückfallen lassen.
- [ ] Oberflächentexte, Validierungsfehler, Rollen, PWA-Metadaten und CSV-Spalten lokalisieren.
- [ ] Datum, Uhrzeit und Zahlen kulturabhängig formatieren; Geldbeträge in Euro ausgeben.
- [ ] Wiederverwendbare Controls mit einer gemeinsamen Code-Basis entwickeln:
  - Tabelle/Liste;
  - Toolbar mit Suche, Filter und Sortierung;
  - Pagination;
  - Formular;
  - Tab-Bar.
- [ ] Mehrere Instanzen desselben Controls auf einer Seite unabhängig betreibbar machen.
- [ ] Einheitliches Icon-, Farb-, Abstands- und Typografie-System definieren.
- [ ] Button-Reihenfolge standardisieren: positiv nach negativ, z. B. `Speichern`, `Zurück`, Abstand, `Löschen`.
- [ ] Touch-Ziele, Kontrast, Screenreader-Texte und Tastaturbedienung berücksichtigen.
- [ ] Lade-, Leer-, Fehler- und Offline-Zustände gestalten.

## 7. Verkaufsoberfläche (MVP-Kern)

- [ ] Zugewiesene Verkaufsliste beim Öffnen auswählen bzw. automatisch laden.
- [ ] Wechsel der aktiven Verkaufsliste jederzeit direkt aus der Verkaufsansicht ermöglichen.
- [ ] Bei genau einer zugewiesenen Verkaufsliste die Auswahl überspringen und diese direkt öffnen.
- [ ] Warenkörbe je Verkaufsliste getrennt offline speichern, damit ein Listenwechsel keine Positionen vermischt oder verwirft.
- [ ] Kachel-Modus mit zweispaltigen, großen Produktbuttons entwickeln.
- [ ] Listen-Modus mit kompakter Produktdarstellung und großem Plus-Feld entwickeln.
- [ ] Umschalter zwischen Kachel- und Listen-Modus implementieren und Präferenz lokal merken.
- [ ] Jedes Antippen sofort und verzögerungsfrei zum Produktzähler addieren.
- [ ] Pfandberechnung und Pfandrückgabe als klar unterscheidbare Schnellauswahl unterstützen; Rückgaben vom Endbetrag abziehen.
- [ ] Sichere Möglichkeit zum Reduzieren/Entfernen versehentlich gewählter Positionen ergänzen.
- [ ] Gesamtsumme oben dauerhaft als großes Taschenrechner-Display anzeigen.
- [ ] Unten einen fixierten Bezahl-Balken mit wiederholter Summenanzeige umsetzen.
- [ ] Bezahlfenster durch Antippen des Summen-/Bezahlbereichs öffnen.
- [ ] Eigenes großes Nummernfeld ohne Systemtastatur umsetzen.
- [ ] Schnellwahltasten für 5, 10, 20 und 50 Euro umsetzen.
- [ ] Barzahlung als einzige auswählbare Zahlungsart des ersten Releases umsetzen; keine funktionslosen Kartenoptionen anzeigen.
- [ ] Rückgeld sofort, kontrastreich und unübersehbar anzeigen.
- [ ] Bei negativem Gesamtbetrag statt des normalen Bezahlvorgangs einen klar gekennzeichneten Auszahlungsdialog ohne Geldschein-Schnellwahl anzeigen.
- [ ] Barauszahlung bestätigen und als Kassenabgang unveränderbar speichern.
- [ ] Abschluss nur bei gültiger Zahlung ermöglichen; optionale exakte Zahlung anbieten.
- [ ] Verkauf atomar lokal speichern und danach alle Zähler zurücksetzen.
- [ ] Schutz gegen versehentliches doppeltes Abschließen einbauen.
- [ ] Bedienbarkeit mit einer Hand auf üblichen Smartphone-Größen testen.

## 8. Verkaufslisten- und Produktverwaltung

- [ ] CRUD-Liste für Verkaufslisten mit Toolbar oberhalb der Tabelle erstellen.
- [ ] Separate Seiten zum Erstellen und Bearbeiten einer Verkaufsliste erstellen.
- [ ] Produktverwaltung je Verkaufsliste mit separaten Bearbeitungsseiten umsetzen.
- [ ] Produktfelder mindestens für Name, Endpreis, Status, Farbe/Icon und Sortierung definieren; Preismodell für spätere Steuerangaben erweiterbar halten.
- [x] Lebenszyklus festgelegt: Benutzer, Produkte und Verkaufslisten können deaktiviert/archiviert werden; endgültiges Löschen ist ausschließlich bei noch nie fachlich verwendeten Datensätzen möglich.
- [ ] Verkaufspositionen mit unveränderlichen Snapshots relevanter Produktdaten wie Name, Preis und Positionstyp speichern.
- [ ] Listenaktionen unterhalb der Liste linksbündig platzieren; Löschen optisch absetzen.
- [ ] Pagination nur direkt unterhalb der Tabelle platzieren.
- [ ] Zuweisung von Verkaufslisten an Benutzer durch Admins umsetzen.
- [ ] Mehrfachzuweisung von Verkaufslisten an Benutzer unterstützen.
- [ ] Bearbeitungsrechte für Verwalter serverseitig erzwingen.
- [ ] Änderungen offline-freundlich versionieren bzw. synchronisierbar machen.

## 9. Benutzerverwaltung

- [ ] Benutzerliste mit Suche, Filter, Sortierung und Rollenfilter erstellen.
- [ ] Separate Seite zum Erstellen eines Benutzers erstellen.
- [ ] Separate Seite zum Bearbeiten, Aktivieren und Deaktivieren erstellen.
- [ ] Rollen und Verkaufslisten-Zuweisungen pflegbar machen.
- [ ] Passwort setzen/zurücksetzen und Session-Widerruf implementieren.
- [ ] Deaktivieren/Archivieren als Standard anbieten; endgültiges Löschen nur für unbenutzte Datensätze als getrennte, bestätigungspflichtige Aktion zulassen.

## 10. Historie und Kassensturz

- [ ] Optionalen Schichtstart mit Eingabe des Anfangsbestands umsetzen.
- [ ] Optionalen Schichtabschluss mit gezähltem Bargeldbestand, Sollbestand und Differenz umsetzen.
- [ ] Verkäufe ohne aktive Schicht weiterhin erlauben und entsprechend kennzeichnen.
- [ ] Historienliste mit Datumsbereich, Benutzer, Verkaufsliste und Produkt filterbar machen.
- [ ] Suche, Sortierung und Pagination oberhalb/unterhalb gemäß UI-Vorgaben platzieren.
- [ ] Detailseite für einen Verkauf mit Positionen, Zahlung und Rückgeld erstellen.
- [ ] Tagesübersicht mit Gesamtumsatz und Produktmengen entwickeln.
- [ ] Einnahmen, Pfandeinnahmen, Pfandrückgaben/Barauszahlungen und Stornos im Kassensturz getrennt ausweisen.
- [ ] Storno als unveränderbare, mit dem Ursprungsverkauf verknüpfte Gegenbuchung samt Auditspur umsetzen.
- [ ] Kassensturz und Export um getrennte Pfandeinnahmen und Pfandrückgaben ergänzen.
- [ ] Zugriff auf Historie und Kassensturz rollenbasiert beschränken.
- [ ] Serverseitig sicherstellen, dass Benutzer ausschließlich eigene Verkäufe abrufen und exportieren können.
- [ ] Serverseitig sicherstellen, dass Verwalter Historien und Exporte ausschließlich für ihre zugewiesenen Verkaufslisten abrufen können.
- [ ] Lokale, noch nicht synchronisierte Verkäufe verständlich kennzeichnen.

## 11. Export und Teilen

- [ ] CSV-Format für Excel-Kompatibilität in deutscher Umgebung festlegen und testen.
- [ ] Export nach Zeitraum, Verkaufsliste und Benutzer ermöglichen.
- [ ] CSV mit Verkäufen und Produktmengen erzeugen.
- [ ] Teilen über Web Share API für unterstützte Mobilgeräte integrieren.
- [ ] Fallback als Dateidownload sowie Übergabe an E-Mail/WhatsApp vorsehen.
- [ ] Offline erzeugten Export lokal bereitstellen und bei Netzrückkehr teilbar machen.

## 12. Qualitätssicherung

- [ ] Unit-Tests für Summen, Rückgeld, Rechte, Versionierung und Synchronisationslogik schreiben.
- [ ] Integrationstests für API, SQLite, Sessions und Autorisierung schreiben.
- [ ] End-to-End-Tests für Verkauf, Zahlung, Abschluss, Historie und Verwaltung erstellen.
- [ ] Tests für doppelte/unterbrochene Synchronisation und Container-Neustarts ergänzen.
- [ ] PWA-/Service-Worker-Updatepfad testen, ohne lokale Verkäufe zu verlieren.
- [ ] Mobile Tests auf kleinen und großen Viewports durchführen.
- [ ] Barrierefreiheit, Kontrast und Touch-Zielgrößen prüfen.
- [ ] Sicherheitsprüfung für Anmeldung, Rollen, Tokens, Eingaben und Exporte durchführen.
- [ ] Sicherung und Wiederherstellung des vollständigen Docker-Volumes dokumentieren und testen; keine automatische Backup-Funktion in TabSale einbauen.

## 13. Release und Betrieb

- [ ] Buildnummer und Builddatum automatisiert in App und Artefakte übernehmen.
- [ ] Development- und Release-Builds klar sichtbar unterscheiden.
- [ ] Release-Checkliste mit Migration, Backup, Rollback und Smoke-Test erstellen.
- [ ] Container-Image reproduzierbar bauen und mit Version taggen.
- [ ] Erstes MVP auf `dev` abnehmen.
- [ ] Freigegebenen Stand nach `main` übernehmen und versionieren.
- [ ] Betriebs- und Wiederherstellungsdokumentation fertigstellen.

## Vorgeschlagene Meilensteine

- [ ] **M0 – Entscheidungen & Fundament:** Architektur, UX-Prototyp, Repository, Docker und Datenmodell.
- [ ] **M1 – Offline-Kassen-MVP:** PWA, Verkauf in beiden Modi, Bezahlfenster und lokale Speicherung.
- [ ] **M2 – Server & Sicherheit:** SQLite, Synchronisation, Anmeldung, Sessions und Rollen.
- [ ] **M3 – Verwaltung:** Benutzer, Verkaufslisten, Produkte und Zuweisungen.
- [ ] **M4 – Auswertung:** Historie, Kassensturz, CSV und Teilen.
- [ ] **M5 – Produktionsreife:** Tests, Sicherheit, Backup, Dokumentation und Release.
