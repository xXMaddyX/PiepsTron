using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using PTSystem;

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
var window = new System.Windows.Window{Height = 800, Width = 1440, Name = "PiepsTron"};
var Webview = new Microsoft.Web.WebView2.Wpf.WebView2();
var PiepsIPC = new PTipcBridge(Webview);
window.Content = Webview;

window.Closing += (sender, e) => {
    App.StopAsync();
};

window.Loaded += async (sender, e) => {
    Webview.Source = new Uri(PTBackend.Url);
    await Webview.EnsureCoreWebView2Async();
    PiepsIPC.InitReceiver();
    
    PiepsIPC.AddReceiver("testsignal", (data) => { Console.WriteLine(data); });
};


WindowApplication.Run(window);

