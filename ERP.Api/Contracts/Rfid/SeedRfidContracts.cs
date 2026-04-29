namespace ERP.Api.Contracts.Rfid;

public sealed class SeedRfidRequest
{
    public Guid CompanyPublicId { get; set; }
    public Guid WarehousePublicId { get; set; }
}

public sealed class SeedRfidResponse
{
    public int OperatorsCreated { get; set; }
    public int CredentialsCreated { get; set; }
    public int SessionsCreated { get; set; }
}
