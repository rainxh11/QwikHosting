namespace QwikHosting.Deno;

public enum DenoBinaryTypePriority
{
    TryPathThenDownloaded,
    UseDownloadedOnly,
    UsePathOnly
}