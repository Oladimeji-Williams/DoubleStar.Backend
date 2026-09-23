// SharedKernel/Domain/Entity.cs — replace the whole file
namespace DoubleStar.SharedKernel.Domain;

public abstract class Entity : IAuditable
{
    public int Id { get; protected set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime ModifiedAt { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    public void Delete()
    {
        if (IsDeleted) return;
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
    }

    public void Restore()
    {
        if (!IsDeleted) return;
        IsDeleted = false;
        DeletedAt = null;
    }
}