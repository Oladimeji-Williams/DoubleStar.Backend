// Domain/Entities/Category.cs
using DoubleStar.SharedKernel.Domain;

namespace DoubleStar.Modules.Catalog.Domain.Entities;

public sealed class Category : Entity
{
    public string Name { get; private set; } = null!;

    private Category() { }

    public static Category Create(string name) => new() { Name = name.Trim() };
}