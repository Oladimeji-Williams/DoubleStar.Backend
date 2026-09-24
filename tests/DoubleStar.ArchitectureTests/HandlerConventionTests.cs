// HandlerConventionTests.cs + SharedKernelPurityTests.cs (two classes, one file is fine)
using System.Reflection;
using NetArchTest.Rules;

namespace DoubleStar.ArchitectureTests;

public sealed class HandlerConventionTests
{
    [Theory]
    [MemberData(nameof(ModuleAssemblies.All), MemberType = typeof(ModuleAssemblies))]
    public void Command_and_query_handlers_should_be_sealed(string moduleName, Assembly assembly)
    {
        var result = Types.InAssembly(assembly)
            .That().HaveNameEndingWith("Handler").And().AreClasses()
            .Should().BeSealed()
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            $"Handlers in {moduleName} should be sealed, but these aren't: " +
            string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? []));
    }
}

public sealed class SharedKernelPurityTests
{
    [Fact]
    public void SharedKernel_should_not_depend_on_ef_core_or_aspnetcore()
    {
        var assembly = typeof(DoubleStar.SharedKernel.Domain.Entity).Assembly;

        var result = Types.InAssembly(assembly)
            .Should().NotHaveDependencyOnAny("Microsoft.EntityFrameworkCore", "Microsoft.AspNetCore")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "SharedKernel must stay framework-agnostic, but depends on: " +
            string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? []));
    }
}