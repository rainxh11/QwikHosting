using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace QwikHosting.Deno;

public static class WebApplicationExtensions
{
    /// <summary>
    /// This will map two forwarding endpoints '/' and '/app' to Deno server
    /// <list type="bullet">
    /// <item>make sure to register YARP by calling <code>builder.Services.AddReverseProxy()</code></item>
    /// <item>make sure to call this after mapping your webapi endpoints</item>
    /// <item>make sure to not map the '/app' endpoint</item>
    /// </list>
    /// </summary>
    /// <param name="app"></param>
    /// <exception cref="Exception"></exception>
    public static void UseQwikDenoReverseProxy(this WebApplication? app)
    {
        var options = app.Services.GetService<IOptions<QwikDenoOptions>>()?.Value;
        var helper = app.Services.GetService<DenoHelper>();
        if (options is null) throw new Exception("Options not found! please use 'builder.Services.AddQwikHosting()'");
        //app.MapForwarder("{**catch-all}", $"http://localhost:{options.Port}");
        foreach (var path in helper.GetClientFolderUrlPaths())
        {
            app.MapForwarder(path, $"http://localhost:{options.Port}");
        }
        app.MapForwarder("", $"http://localhost:{options.Port}");
        app.MapForwarder("/app/{**catch-all}", $"http://localhost:{options.Port}");
    }
}