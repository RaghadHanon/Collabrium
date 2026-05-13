namespace Infrastructure.Reports.Common;

public class ReportBrandingProvider : IReportBrandingProvider
{
    private readonly byte[] _logoBytes;

    public ReportBrandingProvider()
    {
        var path = Path.Combine(
            AppContext.BaseDirectory,
            "Reports",
            "Assets",
            "logo.png");

        if (!File.Exists(path))
            throw new FileNotFoundException("Report logo file was not found.", path);

        _logoBytes = File.ReadAllBytes(path);
    }

    public Task<byte[]> GetLogoAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_logoBytes);
    }
}