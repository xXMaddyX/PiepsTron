# PiepsTron

> 🚧 Frühe Entwicklungsphase — Architektur steht, UI ist aktuell noch Platzhalter.

PiepsTron ist eine Windows-Desktop-Anwendung, die UI und Backend wie bei
**Electron** trennt — nur auf Basis von .NET statt Chromium/Node:

- **Shell/Host:** WPF
- **UI-Renderer:** [WebView2](https://developer.microsoft.com/microsoft-edge/webview2/) (Microsoft Edge / Chromium)
- **Backend:** eingebetteter ASP.NET Core Webserver (Kestrel)

Die App startet lokal einen eigenen Webserver und zeigt dessen Oberfläche
danach in einem nativen Fenster an.

## Tech Stack

| Bereich   | Technologie                          |
|-----------|---------------------------------------|
| Runtime   | .NET 10 (`net10.0-windows`)           |
| UI-Shell  | WPF                                   |
| UI-Render | WebView2                              |
| Backend   | ASP.NET Core / Kestrel                |
| Frontend  | HTML / CSS / JavaScript (ES-Module)   |

## Projektstruktur

```
Program.cs                 App-Einstiegspunkt (Backend-Start + WPF-Fenster)
BackendServer/
  PTBackend.cs              Aufbau & Start des eingebetteten Webservers
src/                        Web-Frontend (HTML/CSS/JS), als WebRoot ausgeliefert
```

## Voraussetzungen

- Windows
- [.NET SDK 10](https://dotnet.microsoft.com/download)
- Microsoft Edge WebView2 Runtime (auf aktuellen Windows-Systemen i. d. R. vorinstalliert)

## Getting Started

```powershell
dotnet build
dotnet run
```

Die App startet den Backend-Server unter `http://localhost:4040` und öffnet
ihn in einem eigenen Fenster.

## Roadmap

- [ ] Eigentliche PiepsTron-UI (aktueller Frontend-Inhalt ist nur Platzhalter)
- [ ] IPC zwischen WPF-Host und WebView2-Frontend (analog zu Electrons
      Main ↔ Renderer-Kommunikation)
- [ ] CORS-Policy für Produktion einschränken
- [ ] Tests

## License

TBD
