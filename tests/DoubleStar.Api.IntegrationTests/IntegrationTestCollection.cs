// IntegrationTestCollection.cs — one container, shared across every test class below
namespace DoubleStar.Api.IntegrationTests;

[CollectionDefinition("Integration")]
public sealed class IntegrationTestCollection : ICollectionFixture<DoubleStarApiFactory>;