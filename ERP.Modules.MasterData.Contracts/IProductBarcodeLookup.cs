namespace ERP.Modules.MasterData.Contracts;

public sealed record ProductBarcodeMatch(long ProductId, string Barcode);

public interface IProductBarcodeLookup
{
    Task<string?> GetBarcodeByProductIdAsync(long companyId, long productId, CancellationToken cancellationToken = default);
    Task<ProductBarcodeMatch?> GetMatchByBarcodeAsync(long companyId, string barcode, CancellationToken cancellationToken = default);
}
