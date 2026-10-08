using System.Net;
using Drydock.Testcontainers.Vault.Tests.Fixtures;
using Drydock.Testcontainers.Vault.Tests.Support;

namespace Drydock.Testcontainers.Vault.Tests.Integration;

public sealed class VaultContainerTest(DefaultVaultFixture fixture) : IClassFixture<DefaultVaultFixture>
{
    private readonly VaultContainer _container = fixture.Container;

    [Fact]
    public async Task ServerIsInitializedAndUnsealedAfterStart()
    {
        using var api = VaultApi.For(_container);

        var health = await api.GetAsync("v1/sys/health", TestContext.Current.CancellationToken);

        Assert.True(health.GetProperty("initialized").GetBoolean());
        Assert.False(health.GetProperty("sealed").GetBoolean());
    }

    [Fact]
    public async Task RootTokenWritesAndReadsSecret()
    {
        using var api = VaultApi.For(_container);

        await api.WriteSecretAsync("orders", "connection", "Host=db;Password=demo", TestContext.Current.CancellationToken);
        var value = await api.ReadSecretAsync("orders", "connection", TestContext.Current.CancellationToken);

        Assert.Equal("Host=db;Password=demo", value);
    }

    [Fact]
    public async Task UnknownTokenIsRejected()
    {
        using var api = new VaultApi(_container.GetBaseAddress(), "not-the-root-token");

        var status = await api.GetStatusAsync("v1/secret/data/orders", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, status);
    }

    [Fact]
    public void RootTokenDefaultsToTheDocumentedConstant()
    {
        Assert.Equal(VaultBuilder.DefaultRootToken, _container.GetRootToken());
    }

    [Fact]
    public void BaseAddressUsesTheMappedHostPort()
    {
        var address = new Uri(_container.GetBaseAddress());

        Assert.Equal(Uri.UriSchemeHttp, address.Scheme);
        Assert.Equal(_container.Hostname, address.Host);
        Assert.Equal(_container.GetMappedPublicPort(VaultBuilder.VaultPort), address.Port);
    }

    [Fact]
    public void ConnectionStringEqualsBaseAddress()
    {
        Assert.Equal(_container.GetBaseAddress(), _container.GetConnectionString());
    }

    [Fact]
    public async Task CliInsideTheContainerIsAuthenticated()
    {
        var result = await _container.ExecAsync(["vault", "token", "lookup", "-format=json"], TestContext.Current.CancellationToken);

        Assert.Equal(0, result.ExitCode);
        Assert.Contains("\"root\"", result.Stdout);
    }
}
