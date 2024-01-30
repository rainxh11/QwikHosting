using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace QwikHosting.Deno;

public static class DependencyExtensions
{
    #if IS_WINDOWS
    public static IServiceCollection AddQwikHosting(this IServiceCollection services,
                                                    Action<QwikDenoOptions>? configureOptions = null)
    {
        return services
              .AddSingleton<IOptions<QwikDenoOptions>>((_) =>
                                                       {
                                                           var options = new QwikDenoOptions();
                                                           configureOptions?.Invoke(options);
                                                           var config =  Options.Create(options);
                                                           return config;
                                                       })
              .AddHostedService<DenoDownloaderService>()
              .AddHostedService<QwikDenoServerRunner>()
              .AddSingleton<DenoHelper>();
    }
    #endif
}