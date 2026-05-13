
namespace Infrastructure.Reports.Common;

public interface IReportBrandingProvider
{
    Task<byte[]> GetLogoAsync(CancellationToken cancellationToken = default);
}