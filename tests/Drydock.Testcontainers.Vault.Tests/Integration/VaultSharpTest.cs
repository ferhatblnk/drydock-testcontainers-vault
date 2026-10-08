using Drydock.Testcontainers.Vault.Tests.Support;
using VaultSharp;
using VaultSharp.V1.AuthMethods.Token;

namespace Drydock.Testcontainers.Vault.Tests.Integration;

public sealed class VaultSharpTest : IAsyncLifetime
{
    private readonly VaultContainer _vault = new VaultBuilder(TestImages.Vault)
        .WithInitCommand("kv put secret/orders-api connection=Host=db")
        .Build();

    public async ValueTask InitializeAsync()
    {
        await _vault.StartAsync(TestContext.Current.CancellationToken);
    }

    public ValueTask DisposeAsync()
    {
        return _vault.DisposeAsync();
    }

    [Fact]
    public async Task ClientReadsTheSeededSecret()
    {
        var client = new VaultClient(new VaultClientSettings(_vault.GetBaseAddress(), new TokenAuthMethodInfo(_vault.GetRootToken())));

        var secret = await client.V1.Secrets.KeyValue.V2.ReadSecretAsync("orders-api", mountPoint: "secret");

        Assert.Equal("Host=db", secret.Data.Data["connection"].ToString());
    }

    [Fact]
    public async Task ClientReadsWhatItWrote()
    {
        var client = new VaultClient(new VaultClientSettings(_vault.GetBaseAddress(), new TokenAuthMethodInfo(_vault.GetRootToken())));
        var data = new Dictionary<string, object> { ["api-key"] = "demo" };

        await client.V1.Secrets.KeyValue.V2.WriteSecretAsync("payments-api", data, mountPoint: "secret");
        var secret = await client.V1.Secrets.KeyValue.V2.ReadSecretAsync("payments-api", mountPoint: "secret");

        Assert.Equal("demo", secret.Data.Data["api-key"].ToString());
    }
}
