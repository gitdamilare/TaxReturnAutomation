namespace TaxReturnAutomation.Infrastructure.Extensions;
public static class UriExtensions
{
    public static string GetFileName(this Uri uri)
    {
        ArgumentNullException.ThrowIfNull(uri);
        return Path.GetFileName(uri.LocalPath);
    }
}
