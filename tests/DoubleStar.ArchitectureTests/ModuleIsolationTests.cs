// ModuleIsolationTests.cs
using System.Reflection;
using NetArchTest.Rules;

namespace DoubleStar.ArchitectureTests;

public sealed class ModuleIsolationTests
{
    [Theory]
    [MemberData(nameof(ModuleAssemblies.All), MemberType = typeof(ModuleAssemblies))]
    public void A_module_should_not_depend_on_any_other_module(string moduleName, Assembly assembly)
    {
        var ownNamespace = $"DoubleStar.Modules.{moduleName}";
        var forbidden = ModuleAssemblies.AllNamespaces.Where(ns => ns != ownNamespace).ToArray();

        var result = Types.InAssembly(assembly)
            .Should()
            .NotHaveDependencyOnAny(forbidden)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            $"{moduleName} should only reach other modules through SharedKernel.Contracts, but depends on: " +
            string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? []));
    }
}