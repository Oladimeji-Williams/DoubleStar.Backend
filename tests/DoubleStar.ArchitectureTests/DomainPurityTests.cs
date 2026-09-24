// DomainPurityTests.cs
using System.Reflection;
using NetArchTest.Rules;

namespace DoubleStar.ArchitectureTests;

public sealed class DomainPurityTests
{
    [Theory]
    [MemberData(nameof(ModuleAssemblies.All), MemberType = typeof(ModuleAssemblies))]
    public void Domain_layer_should_not_depend_on_infrastructure_or_frameworks(string moduleName, Assembly assembly)
    {
        var result = Types.InAssembly(assembly)
            .That().ResideInNamespaceContaining(".Domain")
            .Should().NotHaveDependencyOnAny(
                "Microsoft.EntityFrameworkCore",
                "MediatR",
                "FluentValidation",
                "Microsoft.AspNetCore",
                $"DoubleStar.Modules.{moduleName}.Persistence",
                $"DoubleStar.Modules.{moduleName}.Infrastructure",
                $"DoubleStar.Modules.{moduleName}.Api")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            $"{moduleName}'s Domain layer should be framework-free, but depends on: " +
            string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? []));
    }
}