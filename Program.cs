using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

Thread.CurrentThread.SetApartmentState(ApartmentState.Unknown);
Thread.CurrentThread.SetApartmentState(ApartmentState.STA);


var MyAllowSpecsCors = "_myAllowSpecsCors";
var options = new WebApplicationOptions{
    Args = args,
    WebRootPath = Path.Combine(AppContext.BaseDirectory, "src"),
};

var Builder = WebApplication.CreateBuilder(options);
Builder.Services.AddCors(options => {
    options.AddPolicy(MyAllowSpecsCors, policy => {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

var App = Builder.Build();
App.UseStaticFiles();
App.UseCors(MyAllowSpecsCors);
App.Urls.Add("http://localhost:4040");

App.MapGet("/", async ctx => {
    try {
        using var openFile = File.Open(Path.Combine(AppContext.BaseDirectory, "src/index.html"), mode: FileMode.Open, access: FileAccess.Read);
        var buffer = new byte[openFile.Length];
        await openFile.ReadExactlyAsync(buffer);
        await ctx.Response.WriteAsync(System.Text.Encoding.UTF8.GetString(buffer));
    } catch (Exception ex) {
        Console.WriteLine(ex);
        await ctx.Response.WriteAsync("ERROR 500");
    }
});

void RunServer() {
    Task.Run(() => {App.Run();});
}
RunServer();

var WindowApplication = new System.Windows.Application();
var window = new System.Windows.Window{Height = 800, Width = 1440, Name = "PiepsTron"};
var Webview = new Microsoft.Web.WebView2.Wpf.WebView2();
window.Content = Webview;

window.Closing += (sender, e) => {
    App.StopAsync();
};
window.Loaded += async (sender, e) => {
    Webview.Source = new Uri("http://localhost:4040");
    await Webview.EnsureCoreWebView2Async();
};

WindowApplication.Run(window);