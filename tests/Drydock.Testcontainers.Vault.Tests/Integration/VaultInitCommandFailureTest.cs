using System.Net;
using Drydock.Testcontainers.Vault.Tests.Support;

namespace Drydock.Testcontainers.Vault.Tests.Integration;

public sealed class VaultInitCommandFailureTest
{
    private const string UnknownEngine = "no-such-engine";

    [Fact]
    public async Task StartFailsFastAndSkipsTheRemainingCommands()
    {
        await using var container = new VaultBuilder(TestImages.Vault)
            .WithInitCommand("kv put secret/before step=one", $"secrets enable {UnknownEngine}", "kv put secret/after step=three")
            .Build();

        var exception = await Assert.ThrowsAsync<VaultInitCommandException>(() => container.StartAsync(TestContext.Current.CancellationToken));

        using var api = VaultApi.For(container);
        var before = await api.GetStatusAsync("v1/secret/data/before", TestContext.Current.CancellationToken);
        var after = await api.GetStatusAsync("v1/secret/data/after", TestContext.Current.CancellationToken);

        Assert.NotEqual(0, exception.ExitCode);
        Assert.Contains(UnknownEngine, exception.Stderr);
        Assert.Contains(UnknownEngine, exception.Message);
        Assert.Equal(HttpStatusCode.OK, before);
        Assert.Equal(HttpStatusCode.NotFound, after);
    }
}
