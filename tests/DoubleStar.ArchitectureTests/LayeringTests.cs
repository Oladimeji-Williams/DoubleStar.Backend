using System.Reflection;
using NetArchTest.Rules;

namespace DoubleStar.ArchitectureTests;

public sealed class LayeringTests
{
    [Theory]
    [MemberData(nameof(ModuleAssemblies.All), MemberType = typeof(ModuleAssemblies))]
    public void Application_layer_should_not_depend_on_outer_layers(string moduleName, Assembly assembly)
    {
        var result = Types.InAssembly(assembly)
            .That().ResideInNamespaceContaining(".Application")
            .Should().NotHaveDependencyOnAny(
                "Microsoft.EntityFrameworkCore",
                "Microsoft.AspNetCore",
                $"DoubleStar.Modules.{moduleName}.Persistence",
                $"DoubleStar.Modules.{moduleName}.Infrastructure",
                $"DoubleStar.Modules.{moduleName}.Api")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            $"{moduleName}'s Application layer must depend only on Domain and abstractions, but depends on: " +
            string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? []));
    }

    [Theory]
    [MemberData(nameof(ModuleAssemblies.All), MemberType = typeof(ModuleAssemblies))]
    public void Api_layer_should_not_reach_into_persistence_or_infrastructure(string moduleName, Assembly assembly)
    {
        var result = Types.InAssembly(assembly)
            .That().ResideInNamespaceContaining(".Api")
            .Should().NotHaveDependencyOnAny(
                $"DoubleStar.Modules.{moduleName}.Persistence",
                $"DoubleStar.Modules.{moduleName}.Infrastructure")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            $"{moduleName}'s controllers should go through MediatR, but depend on: " +
            string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? []));
    }
}