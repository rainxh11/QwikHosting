namespace QwikHosting.Deno;

public record QwikDenoOptions
{
    public QwikDenoOptions()
    {
        DenoDownloadDir = Path.Combine(AppContext.BaseDirectory, "Deno");
        BaseDirectory = Path.Combine(AppContext.BaseDirectory, "Qwik");
        EntryPointRelativePath = @"server\entry.deno.js";
        Port = Random.Shared.Next(10_000, 20_000);
        BinaryPick = DenoBinaryTypePriority.TryPathThenDownloaded;
    }

    public string DenoDownloadDir { get; set; }
    public string BaseDirectory { get; set; }
    public string EntryPointRelativePath { get; set; }
    public int Port { get; set; }
    public DenoBinaryTypePriority BinaryPick { get; set; }
}