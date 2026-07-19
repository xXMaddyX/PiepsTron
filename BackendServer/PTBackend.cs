using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Cors.Infrastructure;

namespace PTSystem;

/// <summary>
/// Wraps a self-hosted ASP.NET Core (Kestrel) web server that serves the local
/// frontend content. This is PiepsTron's equivalent of Electron's embedded
/// backend process: it runs the app's "main" logic on localhost and hands the
/// resulting <see cref="WebApplication"/> back so a native shell (e.g. a
/// WebView2 control) can point at it.
/// </summary>
public class PTBackendHandler{
    /// <summary>The built and running backend instance, or <c>null</c> before <see cref="InitBackend"/> has been called.</summary>
    public WebApplication? PiepsTronBackend { get; set; } = null;

    /// <summary>The base URL the backend listens on (e.g. <c>http://localhost:4040</c>).</summary>
    public string Url { get; private set; } = "http://localhost:3030";

    /// <summary>Fallback CORS policy name used when the caller doesn't supply their own via <see cref="InitBackend"/>.</summary>
    private string _defaultAllowSpecsCors { get; } = "_myAllowSpecsCors";
    
    /// <summary>Name of the CORS policy actually in use, set once a caller-supplied policy name is provided.</summary>
    private string? AllowSpecsCors { get; set; }
//###############################################################################################################
    /// <summary>
    /// Builds and starts the backend web server.
    /// </summary>
    /// <param name="url">URL to listen on. Required together with <paramref name="webOptions"/> and <paramref name="setupAction"/> to use a custom configuration; if any of the three is <c>null</c>, a basic default configuration is used instead.</param>
    /// <param name="webOptions">Options passed to <see cref="WebApplication.CreateBuilder(WebApplicationOptions)"/>, e.g. the static file web root.</param>
    /// <param name="setupAction">CORS policy configuration for the server's <see cref="CorsOptions"/>.</param>
    /// <param name="corsPolicyName">Name under which <paramref name="setupAction"/>'s policy is registered and applied. Falls back to an internal default name when <c>null</c>.</param>
    /// <returns>The running <see cref="WebApplication"/> instance.</returns>
    public WebApplication InitBackend(string? url ,WebApplicationOptions? webOptions, Action<CorsOptions>? setupAction, string? corsPolicyName = null) {
        if (webOptions != null && setupAction != null && url != null) {
            Url = url;
            AllowSpecsCors = corsPolicyName;
            SetUserOptions(webOptions, setupAction);
        } else { SetBasicOptions(); };
        StartServer(Url);
        return PiepsTronBackend!;
    }
//###############################################################################################################
    /// <summary>
    /// Runs the built backend on a background task so it doesn't block the caller
    /// (in particular, the WPF UI thread that starts it).
    /// </summary>
    private void StartServer(string url) {
        if (PiepsTronBackend != null) {
            Task.Run(() => {
                PiepsTronBackend.Run(url);
            });
        } else {
            Console.WriteLine("ERROR while starting App Backend!!!");
        }
    }
//###############################################################################################################
    /// <summary>Configures the backend with default options: web root under <c>src/</c> next to the executable and a permissive CORS policy.</summary>
    private void SetBasicOptions() {
        var AppOptions = new WebApplicationOptions{
                WebRootPath = Path.Combine(AppContext.BaseDirectory, "src"),
        };
        var Builder = WebApplication.CreateBuilder(AppOptions);
        Builder.Services.AddCors(options => {
            options.AddPolicy(_defaultAllowSpecsCors, policy => {
                    policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
            });
        });
        BuildServer(Builder);
    }
//###############################################################################################################
    /// <summary>Configures the backend using caller-supplied <see cref="WebApplicationOptions"/> and CORS setup.</summary>
    private void SetUserOptions(WebApplicationOptions webOptions, Action<CorsOptions> setupAction) {
        var Builder = WebApplication.CreateBuilder(webOptions);
            Builder.Services.AddCors(setupAction);
        BuildServer(Builder);
    }
//###############################################################################################################
    /// <summary>Builds the <see cref="WebApplication"/> and wires up static file serving and the active CORS policy.</summary>
    private void BuildServer(WebApplicationBuilder Builder) {
        PiepsTronBackend = Builder.Build();
        PiepsTronBackend.UseStaticFiles();
        if (AllowSpecsCors != null) { PiepsTronBackend.UseCors(AllowSpecsCors); }
        else { PiepsTronBackend.UseCors(_defaultAllowSpecsCors); }
    }
}