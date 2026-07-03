using Xunit;
using NetArchTest.Rules;
using LaundrySaas.Domain.Identity;
using LaundrySaas.Application;
using LaundrySaas.Infrastructure;

namespace LaundrySaas.ArchitectureTests;

public class ArchitectureTests
{
    private const string DomainNamespace = "LaundrySaas.Domain";
    private const string ApplicationNamespace = "LaundrySaas.Application";
    private const string InfrastructureNamespace = "LaundrySaas.Infrastructure";
    private const string ApiNamespace = "LaundrySaas.Api";

    [Fact]
    public void Domain_Should_Not_HaveDependencyOn_OtherProjects()
    {
        // Arrange
        var assembly = typeof(User).Assembly;

        // Act
        var result = Types.InAssembly(assembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                ApplicationNamespace,
                InfrastructureNamespace,
                ApiNamespace)
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful, "Domain layer should not have dependencies on Application, Infrastructure, or Api projects.");
    }

    [Fact]
    public void Application_Should_Not_HaveDependencyOn_InfrastructureOrApi()
    {
        // Arrange
        var assembly = typeof(LaundrySaas.Application.DependencyInjection).Assembly;

        // Act
        var result = Types.InAssembly(assembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                InfrastructureNamespace,
                ApiNamespace)
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful, "Application layer should not have dependencies on Infrastructure or Api projects.");
    }

    [Fact]
    public void Infrastructure_Should_Not_HaveDependencyOn_Api()
    {
        // Arrange
        var assembly = typeof(LaundrySaas.Infrastructure.DependencyInjection).Assembly;

        // Act
        var result = Types.InAssembly(assembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                ApiNamespace)
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful, "Infrastructure layer should not have dependencies on Api project.");
    }
}
