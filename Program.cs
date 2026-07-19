// PiepsTron entry point: starts the embedded ASP.NET Core backend, then opens
// a WPF window with a WebView2 control pointed at it (PiepsTron's take on
// Electron's main-process + renderer split).
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using PTSystem;

// WPF/WebView2 rely on COM and require the UI thread to run in a single-threaded
// apartment. Top-level statements don't support the [STAThread] attribute, so
// the apartment state is set explicitly here instead, before any WPF/COM object
// is created.
Thread.CurrentThread.SetApartmentState(ApartmentState.Unknown);
Thread.CurrentThread.SetApartmentState(ApartmentState.STA);

//###############################################################################################################
//------------------------------------------>>>>INIT_PIEPS_BACKEND<<<<------------------------------------------#
//###############################################################################################################
var MyAllowSpecsCors = "_myAllowSpecsCors";
var options = new WebApplicationOptions{
    Args = args,
    WebRootPath = Path.Combine(AppContext.BaseDirectory, "src"),
};
//###############################################################################################################
var PTBackend = new PTBackendHandler();
var App = PTBackend.InitBackend("http://localhost:4040", options, setup => {
    setup.AddPolicy(MyAllowSpecsCors, poly => {
        poly.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin();
    });
});
//###############################################################################################################
App.MapGet("/", async ctx => {
    try {
        var fileData = await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "src/index.html"));
        await ctx.Response.WriteAsync(fileData);
    } catch (Exception ex) {
        Console.WriteLine(ex);
        await ctx.Response.WriteAsync("ERROR 500");
    }
});
//###############################################################################################################
//----------------------------------------->>>>INIT_WINDOW_APPLICATION<<<<--------------------------------------#
//###############################################################################################################

var WindowApplication = new System.Windows.Application();
var window = new System.Windows.Window{Height = 600, Width = 800, Name = "PiepsTron"};
var Webview = new Microsoft.Web.WebView2.Wpf.WebView2();
var PiepsIPC = new PTipcBridge(Webview);
window.Content = Webview;

window.Closing += (sender, e) => {
    App.StopAsync();
};

// Initialization happens in Loaded rather than inline before WindowApplication.Run():
// WPF's message loop (needed to marshal the `await` continuations below back to
// the UI thread) only starts once Run() is called, so anything awaited earlier
// would never resume. Loaded fires after Run() has started pumping messages.
window.Loaded += async (sender, e) => {
    Webview.Source = new Uri(PTBackend.Url);
    await Webview.EnsureCoreWebView2Async();
    PiepsIPC.InitReceiver();

    PiepsIPC.AddSender("callfrontend");

    // Example channel: frontend sends "testsignal", backend logs it and replies on "callfrontend".
    PiepsIPC.AddReceiver("testsignal", (data) => {
        Console.WriteLine(data);

        PiepsIPC.SendMessage("""{"data": "Message from Backend"}""", "callfrontend");
    });
};


WindowApplication.Run(window);

