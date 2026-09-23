// Domain/Entities/Brand.cs
using DoubleStar.SharedKernel.Domain;

namespace DoubleStar.Modules.Catalog.Domain.Entities;

public sealed class Brand : Entity
{
    public string Name { get; private set; } = null!;

    private Brand() { }

    public static Brand Create(string name) => new() { Name = name.Trim() };
}