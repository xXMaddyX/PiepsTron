# PiepsTron

> ⚠️ Vorläufige Dokumentation — automatisch aus dem aktuellen Code-Stand abgeleitet. Muss noch von Hand geprüft/ergänzt werden.

## Überblick

PiepsTron verfolgt bewusst einen **Electron-artigen Ansatz**: eine
Windows-Desktop-Anwendung (WPF), die eine lokal laufende ASP.NET-Core-Webseite
in einem eingebetteten Browser-Fenster
([WebView2](https://developer.microsoft.com/microsoft-edge/webview2/))
anzeigt. Die Anwendung startet einen eigenen kleinen Webserver und öffnet ihn
danach in einem nativen Fenster — das UI wird also mit Web-Technologien
(HTML/CSS/JS) gebaut, der Host/Shell ist natives .NET/WPF statt
Chromium/Node wie bei Electron. Die geplante IPC-Erweiterung (siehe unten)
entspricht damit konzeptionell Electrons Main↔Renderer-Kommunikation.

Aktuell dient als Frontend-Inhalt eine Demo-Landingpage für einen fiktiven
Malerbetrieb ("**KERN — Meisterhafte Malerei**"). Das ist **nur Platzhalter**
und wird durch die eigentliche PiepsTron-UI ersetzt.

Geplant ist außerdem eine Erweiterung um **IPC** (Kommunikation zwischen der
WPF-Hostanwendung und dem im WebView2 laufenden Web-Frontend/Backend) — dazu
gibt es aktuell noch keine Implementierung im Code.

## Architektur

```
PiepsTron.csproj        WPF-Projekt (net10.0-windows, UseWPF=true)
Program.cs               App-Einstiegspunkt: startet Backend + WPF-Fenster
BackendServer/
  PTBackend.cs            Kapselt Aufbau/Start des ASP.NET-Core-Servers
src/                      Statische Web-Assets (HTML/CSS/JS), als WebRoot
  index.html
  css/...
  js/...
```

### Ablauf beim Start (`Program.cs`)

1. `PTBackendHandler.InitBackend(...)` baut eine `WebApplication`
   (ASP.NET Core / Kestrel) auf, mit `WebRootPath` = `<AppContext.BaseDirectory>/src`.
2. Es wird eine CORS-Policy registriert, die alle Header/Methoden/Origins
   erlaubt (`AllowAnyHeader/Method/Origin`) — aktuell ohne Einschränkung.
3. Der Server wird asynchron unter `http://localhost:4040` gestartet
   (`StartServer` startet `PiepsTronBackend.Run(url)` in einem `Task.Run`).
4. Eine Route `GET /` liest `src/index.html` von der Festplatte und liefert
   sie direkt als Antwort zurück (zusätzlich zu `UseStaticFiles()`, das den
   Rest von `src/` als statische Dateien ausliefert).
5. Parallel wird ein WPF-`Window` (1440×800) mit einem `WebView2`-Control
   erzeugt. Sobald das Fenster geladen ist, wird `Webview.Source` auf die
   Backend-URL gesetzt — die Seite wird also im eingebetteten Browser
   angezeigt.
6. Beim Schließen des Fensters wird der Server über `App.StopAsync()`
   gestoppt.

### `BackendServer/PTBackend.cs`

`PTBackendHandler` ist eine kleine Wrapper-Klasse um `WebApplication`:

- `InitBackend(url, webOptions, setupAction, corsPolicyName)` — Haupteinstieg.
  Wenn `url`/`webOptions`/`setupAction` gesetzt sind, werden benutzerdefinierte
  Optionen verwendet (`SetUserOptions`), sonst Standardwerte (`SetBasicOptions`,
  Default-URL `http://localhost:3030`).
- `BuildServer(builder)` — baut die `WebApplication`, aktiviert
  `UseStaticFiles()` und die konfigurierte CORS-Policy.
- `StartServer(url)` — startet den Server asynchron über `Task.Run`.
- `Url` — die tatsächlich verwendete Basis-URL, wird von `Program.cs`
  ausgelesen, um sie in die `WebView2` zu laden.

**Hinweis:** In `Program.cs` wird die URL `http://localhost:4040` fest
vorgegeben, obwohl `PTBackend.Url` standardmäßig `http://localhost:3030` ist
— die Default-URL im Handler wird also nie benutzt, solange `InitBackend`
immer mit einer expliziten URL aufgerufen wird.

## Frontend (`src/`)

Aktuell eine reine, clientseitig gerenderte HTML/CSS/JS-Landingpage
(kein Framework/Build-Step erkennbar, ES-Module via `<script type="module">`):

- **HTML:** `index.html` — Single-Page-Layout mit Sections: Hero, Marquee,
  About, Services, Portfolio, Process, Testimonials, Contact, Footer.
- **CSS:** modular unter `css/` aufgeteilt in `variables`, `reset`, `base`,
  `animations`, `components/` (cursor, nav, hero, marquee, footer) und
  `sections/` (about, services, portfolio, process, testimonials, contact).
- **JS:** Einstiegspunkt `js/main.js`, feature-basierte Module unter
  `js/modules/`: `animations`, `counter`, `cursor` (custom Cursor-Ring),
  `form` (Kontaktformular), `loader` (Ladebildschirm), `magnetic`
  (magnetische Buttons), `marquee`, `nav`, `portfolio` (Drag-Slider),
  `scrollProgress`, `testimonials` (Slider).

Der Seiteninhalt ("KERN Malerei") ist offensichtlich Platzhalter-/Demo-Content
und referenziert Google Fonts extern (`Playfair Display`, `Inter`,
`Cormorant Garamond`).

## Voraussetzungen

- Windows (WPF + WebView2 sind Windows-only)
- .NET SDK 10.0 (Target Framework `net10.0-windows`)
- Microsoft Edge WebView2 Runtime (i. d. R. auf aktuellen Windows-Systemen
  vorinstalliert)
- NuGet-Paket `Microsoft.Web.WebView2` (bereits im `.csproj` referenziert)

## Bauen & Starten

```powershell
dotnet build
dotnet run
```

Beim Start wird automatisch ein Server auf `http://localhost:4040`
hochgefahren und in einem WPF-Fenster angezeigt.

## Offene Punkte / To-Dos für die finale Doku

- [ ] Klären, ob `src/` versioniert werden soll — aktuell ist der Ordner
  nicht im Git-Repo enthalten, nur die kompilierten Kopien in `bin/`/`obj/`.
  Nur `/bin` steht in `.gitignore`, `/obj` bisher nicht.
- [ ] Zweck/Zielsetzung von PiepsTron als eigentliche Anwendung beschreiben,
  sobald die Platzhalter-Landingpage ("KERN Malerei") ersetzt ist.
- [ ] **IPC-Konzept** definieren und dokumentieren, sobald implementiert:
  z. B. WebView2 `CoreWebView2.WebMessageReceived`/`PostWebMessageAsString`
  für WPF ↔ JS, oder eigene Endpoints im ASP.NET-Core-Backend für
  Web ↔ Backend-Kommunikation. Aktuell existiert dafür noch kein Code.
- [ ] Fehlerbehandlung/Logging im Backend ausbauen (aktuell nur
  `Console.WriteLine` bei Fehlern).
- [ ] CORS-Policy (`AllowAnyOrigin/Header/Method`) für Produktion einschränken.
- [ ] Tests (aktuell keine vorhanden).
