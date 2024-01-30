using System.Collections;
using System.Diagnostics;
using CliWrap;
using Microsoft.Extensions.Options;

namespace QwikHosting.Deno;

internal class DenoHelper(IOptions<QwikDenoOptions> options)
{
    internal List<string> GetClientFolderUrlPaths()
    {
        var clientDir = Path.Combine(options.Value.BaseDirectory, "dist");
        var files = new DirectoryInfo(clientDir)
                   .EnumerateFiles("*", SearchOption.AllDirectories)
                   .Select(x => new Uri(x.FullName.Replace(clientDir, ""), UriKind.Relative))
                   .Select(x => x.ToString().Replace(@"\",@"/"))
                   .ToList();
        return files;

    } 
    internal (string Path, Version version)? GetLatestDownloaded()
    {
        var dir = new DirectoryInfo(options.Value.DenoDownloadDir);
        if (!dir.Exists) return null;
        return dir.EnumerateFiles("deno.exe", SearchOption.AllDirectories)
                  .Select(x => (Path: x.FullName,
                                Version: Version.Parse(FileVersionInfo.GetVersionInfo(x.FullName).FileVersion!)))
                  .MaxBy(x => x.Version);
    }

    private bool DenoFound()
    {
        return Environment.GetEnvironmentVariables()
                          .Cast<DictionaryEntry>()
                          .Select(x => new DirectoryInfo(x.Value as string ?? string.Empty))
                          .Where(x => x.Exists)
                          .SelectMany(x => x.EnumerateFiles("deno.exe", SearchOption.TopDirectoryOnly))
                          .Any();
    }

    internal string? GetDenoBinary()
    {
        switch (options.Value.BinaryPick)
        {
            default:
            case DenoBinaryTypePriority.TryPathThenDownloaded:
                return DenoFound() ? "deno" : GetLatestDownloaded()?.Path;
            case DenoBinaryTypePriority.UsePathOnly:
                return DenoFound() ? "deno" : null;
            case DenoBinaryTypePriority.UseDownloadedOnly:
                return GetLatestDownloaded()?.Path;
        }
    }
}