using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Cors.Infrastructure;

namespace PTBackend;

public class PTBackendHandler{
    public WebApplication? PiepsTronBackend { get; set; } = null;
    public string Url { get; private set; } = "http://localhost:3030";
    private string _defaultAllowSpecsCors { get; } = "_myAllowSpecsCors";
    private string? AllowSpecsCors { get; set; }
//###############################################################################################################
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
    private void SetUserOptions(WebApplicationOptions webOptions, Action<CorsOptions> setupAction) {
        var Builder = WebApplication.CreateBuilder(webOptions);
            Builder.Services.AddCors(setupAction);
        BuildServer(Builder);
    }
//###############################################################################################################
    private void BuildServer(WebApplicationBuilder Builder) {
        PiepsTronBackend = Builder.Build();
        PiepsTronBackend.UseStaticFiles();
        if (AllowSpecsCors != null) { PiepsTronBackend.UseCors(AllowSpecsCors); }
        else { PiepsTronBackend.UseCors(_defaultAllowSpecsCors); }
    }
}