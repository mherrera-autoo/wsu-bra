namespace ERP.EdgePublicApi.Configuration;

public sealed class EdgePublicApiOptions
{
    public const string SectionName = "EdgePublicApi";

    public string? ApiKey { get; set; }
    public Dictionary<string, EdgeNodeOptions> Nodes { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    public bool TryGetNode(string edgeNodeId, out EdgeNodeOptions node)
    {
        var normalized = edgeNodeId.Trim();
        return Nodes.TryGetValue(normalized, out node!);
    }
}

public sealed class EdgeNodeOptions
{
    public Guid CompanyPublicId { get; set; }
    public Guid? WarehousePublicId { get; set; }
    public string[] AllowedZones { get; set; } = [];
    public string? MovementPolicy { get; set; }
    public string[] ReaderIds { get; set; } = [];
    public DateTimeOffset? ValidFromUtc { get; set; }
    public DateTimeOffset? ValidToUtc { get; set; }
}
