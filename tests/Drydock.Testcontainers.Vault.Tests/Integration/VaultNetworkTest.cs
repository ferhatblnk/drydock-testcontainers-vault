using DotNet.Testcontainers.Builders;
using Drydock.Testcontainers.Vault.Tests.Support;

namespace Drydock.Testcontainers.Vault.Tests.Integration;

public sealed class VaultNetworkTest
{
    private const string Alias = "vault";

    [Fact]
    public async Task OtherContainerReachesTheServerThroughItsAlias()
    {
        await using var network = new NetworkBuilder().Build();

        await using var server = new VaultBuilder(TestImages.Vault)
            .WithNetwork(network)
            .WithNetworkAliases(Alias)
            .Build();

        await using var client = new VaultBuilder(TestImages.Vault)
            .WithNetwork(network)
            .Build();

        await Task.WhenAll(server.StartAsync(TestContext.Current.CancellationToken), client.StartAsync(TestContext.Current.CancellationToken));

        var status = await client.ExecAsync(["vault", "status", $"-address=http://{Alias}:{VaultBuilder.VaultPort}"], TestContext.Current.CancellationToken);

        Assert.Equal(0, status.ExitCode);
    }
}
