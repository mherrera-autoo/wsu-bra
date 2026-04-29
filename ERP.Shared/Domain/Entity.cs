namespace ERP.Shared.Domain;

public abstract class Entity
{
    public long Id { get; protected set; }
    public Guid PublicId { get; protected set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; protected set; }
}
