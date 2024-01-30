using System.Diagnostics;
using System.Reactive.Linq;
using CliWrap;
using CliWrap.Buffered;
using CliWrap.EventStream;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace QwikHosting.Deno;

public class QwikDenoServerRunner : BackgroundService
{
    private IOptions<QwikDenoOptions> _options;
    private ILogger<QwikDenoServerRunner> _logger;
    private readonly DenoHelper _helper;

    public QwikDenoServerRunner(ILogger<QwikDenoServerRunner> logger, IOptions<QwikDenoOptions> options,
                                IServiceProvider provider)
    {
        _logger = logger;
        _options = options;
        _helper = provider.GetRequiredService<DenoHelper>();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var deno = _helper.GetDenoBinary();
                if (deno is not null)
                {
                    _logger?.LogInformation("Deno Found {@deno}", deno);
                    var entryPoint = Path.Combine(_options.Value.BaseDirectory, _options.Value.EntryPointRelativePath);
                    if (File.Exists(entryPoint))
                    {
                        _logger?.LogInformation("Qwik server Deno entry point found: {@entry}", entryPoint);
                        await Cli.Wrap(deno)
                                 .WithArguments(new[]
                                                {
                                                    "run",
                                                    "--allow-net",
                                                    "--allow-read",
                                                    "--allow-env",
                                                    entryPoint
                                                })
                                 .WithEnvironmentVariables(x => x.Set("PORT", _options.Value.Port.ToString()))
                                 .ExecuteBufferedAsync();
                    }
                    else _logger?.LogError("Qwik server Deno entry point not found in: {@path}", entryPoint);
                }
                else _logger?.LogError("Deno binary Not Found/Still Downloading!");
            }
            catch (Exception e)
            {
                _logger?.LogError(e.Message);
            }

            await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
        }
        Process.GetProcessesByName("deno.exe")
               .ToList()
               .ForEach(x => x.Kill());
    }
}