// ModuleAssemblies.cs
using System.Reflection;

namespace DoubleStar.ArchitectureTests;

public static class ModuleAssemblies
{
    public static readonly TheoryData<string, Assembly> All = new()
    {
        { "Identity", typeof(DoubleStar.Modules.Identity.AssemblyMarker).Assembly },
        { "Customers", typeof(DoubleStar.Modules.Customers.AssemblyMarker).Assembly },
        { "Catalog", typeof(DoubleStar.Modules.Catalog.AssemblyMarker).Assembly },
        { "Inventory", typeof(DoubleStar.Modules.Inventory.AssemblyMarker).Assembly },
        { "Sales", typeof(DoubleStar.Modules.Sales.AssemblyMarker).Assembly },
        { "Repairs", typeof(DoubleStar.Modules.Repairs.AssemblyMarker).Assembly },
        { "Payments", typeof(DoubleStar.Modules.Payments.AssemblyMarker).Assembly },
        { "Notifications", typeof(DoubleStar.Modules.Notifications.AssemblyMarker).Assembly },
        { "Reporting", typeof(DoubleStar.Modules.Reporting.AssemblyMarker).Assembly },
    };

    public static readonly string[] AllNamespaces =
    [
        "DoubleStar.Modules.Identity",
        "DoubleStar.Modules.Customers",
        "DoubleStar.Modules.Catalog",
        "DoubleStar.Modules.Inventory",
        "DoubleStar.Modules.Sales",
        "DoubleStar.Modules.Repairs",
        "DoubleStar.Modules.Payments",
        "DoubleStar.Modules.Notifications",
        "DoubleStar.Modules.Reporting",
    ];
}