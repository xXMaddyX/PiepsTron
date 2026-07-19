# PiepsTron

> 🚧 Early development — architecture is in place, UI is still a placeholder.

PiepsTron is a Windows desktop app that splits UI and backend the way
**Electron** does — just built on .NET instead of Chromium/Node:

- **Shell/Host:** WPF
- **UI renderer:** [WebView2](https://developer.microsoft.com/microsoft-edge/webview2/) (Microsoft Edge / Chromium)
- **Backend:** embedded ASP.NET Core web server (Kestrel)
- **IPC:** a small named-channel bridge between the WPF host and the WebView2
  frontend, modeled after Electron's `ipcMain`/`ipcRenderer`

The app starts its own local web server and then displays that server's UI
inside a native window.

## Tech Stack

| Area      | Technology                           |
|-----------|---------------------------------------|
| Runtime   | .NET 10 (`net10.0-windows`)           |
| UI shell  | WPF                                   |
| UI render | WebView2                              |
| Backend   | ASP.NET Core / Kestrel                |
| Frontend  | HTML / CSS / JavaScript (ES modules)  |

## Project Structure

```
Program.cs                   App entry point: starts the backend, opens the WPF window + WebView2
BackendServer/
  PTBackend.cs                PTBackendHandler — builds & runs the embedded Kestrel server
  PTIPCBridge.cs               PTipcBridge — named-channel IPC between WPF host and WebView2 frontend
src/                          Web frontend (HTML/CSS/JS), served as the backend's web root
```

## IPC

Channels are identified by name, similar to Electron's IPC channels:

```csharp
// Backend -> Frontend
PiepsIPC.AddSender("callfrontend");
PiepsIPC.SendMessage("""{"data": "Message from Backend"}""", "callfrontend");

// Frontend -> Backend
PiepsIPC.AddReceiver("testsignal", (data) => {
    Console.WriteLine(data);
});
```

```js
// Frontend -> Backend
window.chrome.webview.postMessage({ Name: "testsignal", Data: "hello" });

// Backend -> Frontend
window.chrome.webview.addEventListener("message", (event) => {
    console.log(event.data);
});
```

## Requirements

- Windows
- [.NET SDK 10](https://dotnet.microsoft.com/download)
- Microsoft Edge WebView2 Runtime (preinstalled on most current Windows systems)

## Getting Started

```powershell
dotnet build
dotnet run
```

The app starts the backend server on `http://localhost:4040` and opens it in
its own window.

## Roadmap

- [ ] Actual PiepsTron UI (current frontend content is a placeholder)
- [ ] Linux support (alternative to WebView2, e.g. WebKitGTK)
- [ ] macOS support (alternative to WebView2, e.g. WKWebView)
- [ ] Restrict the CORS policy for production
- [ ] Tests

## License

TBD
