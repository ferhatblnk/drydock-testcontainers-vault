using System.Net;
using Drydock.Testcontainers.Vault.Tests.Fixtures;
using Drydock.Testcontainers.Vault.Tests.Support;

namespace Drydock.Testcontainers.Vault.Tests.Integration;

public sealed class VaultRootTokenTest(CustomTokenVaultFixture fixture) : IClassFixture<CustomTokenVaultFixture>
{
    private readonly VaultContainer _container = fixture.Container;

    [Fact]
    public async Task CustomTokenAuthenticates()
    {
        using var api = VaultApi.For(_container);

        await api.WriteSecretAsync("custom", "key", "value", TestContext.Current.CancellationToken);
        var value = await api.ReadSecretAsync("custom", "key", TestContext.Current.CancellationToken);

        Assert.Equal(CustomTokenVaultFixture.RootToken, _container.GetRootToken());
        Assert.Equal("value", value);
    }

    [Fact]
    public async Task DefaultTokenNoLongerAuthenticates()
    {
        using var api = new VaultApi(_container.GetBaseAddress(), VaultBuilder.DefaultRootToken);

        var status = await api.GetStatusAsync("v1/secret/data/custom", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, status);
    }

    [Fact]
    public async Task CliUsesTheCustomToken()
    {
        var result = await _container.ExecAsync(["vault", "kv", "put", "secret/cli", "from=exec"], TestContext.Current.CancellationToken);

        Assert.Equal(0, result.ExitCode);
    }
}
