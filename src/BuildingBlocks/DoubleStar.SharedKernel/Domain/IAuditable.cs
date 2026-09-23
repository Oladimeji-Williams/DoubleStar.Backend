// SharedKernel/Domain/IAuditable.cs
namespace DoubleStar.SharedKernel.Domain;

public interface IAuditable
{
    DateTime CreatedAt { get; }
    DateTime ModifiedAt { get; }
}