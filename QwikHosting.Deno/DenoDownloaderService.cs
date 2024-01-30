using System.IO.Compression;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SakontStack.ReactiveStream;

namespace QwikHosting.Deno;

public class DenoDownloaderService : BackgroundService
{
    private ILogger<DenoDownloaderService> _logger;
    private readonly QwikDenoOptions _options;
    private readonly DenoHelper _helper;
    private readonly HttpClient _httpClient;

    private const string _latestUrl =
        "https://github.com/denoland/deno/releases/latest/download/deno-x86_64-pc-windows-msvc.zip";

    public DenoDownloaderService(IOptions<QwikDenoOptions> options,
                                 ILogger<DenoDownloaderService> logger,
                                 IServiceProvider provider)
    {
        _options = options.Value;
        _logger = logger;
        _helper = provider.GetRequiredService<DenoHelper>();
        _httpClient = new HttpClient();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_options.BinaryPick is DenoBinaryTypePriority.UsePathOnly ||
            _helper.GetLatestDownloaded().HasValue) return;
        if (!Directory.Exists(_options.DenoDownloadDir)) Directory.CreateDirectory(_options.DenoDownloadDir);
        await DownloadLatest();
    }

    private async Task DownloadLatest()
    {
        try
        {
            using var response = await _httpClient.GetAsync(_latestUrl);
            var length = long.Parse(response.Content.Headers.First(h => h.Key.Equals("Content-Length")).Value.First());
            var progress = new Progress<ReactiveStream.StreamProgress>(p =>
                                                                       {
                                                                           _logger
                                                                             ?.LogInformation("Downloading Deno {@d:N2}MB/{@total}MB ({@percent}%)",
                                                                                        p.Bytes / 1024 / 1024,
                                                                                        p.TotalBytes / 1024 / 1024,
                                                                                        p.Percentage);
                                                                       });
            await using var stream = new ReactiveStream(new MemoryStream(), progress: progress, totalLength: length);
            await response.Content.CopyToAsync(stream);
            using var zipArchive = new ZipArchive(stream);
            _logger?.LogInformation("Extracting Deno...");
            zipArchive.ExtractToDirectory(_options.DenoDownloadDir);
            _logger?.LogInformation("Downloaded Deno version: {@version}",
                                    _helper.GetLatestDownloaded()?.version.ToString());
        }
        catch (Exception e)
        {
            _logger?.LogError(e.ToString());
        }
    }
}