using Drydock.Testcontainers.Vault.Tests.Support;

namespace Drydock.Testcontainers.Vault.Tests.Integration;

public sealed class VaultCompatibilityTest
{
    private const string RootToken = "compatibility-token";

    public static TheoryData<string> Images => new()
    {
        "hashicorp/vault:1.13",
        "hashicorp/vault:1.19",
        TestImages.Vault,
        "openbao/openbao:2.4",
        TestImages.OpenBao,
    };

    [Theory]
    [MemberData(nameof(Images))]
    public async Task StartsAuthenticatesAndRunsInitCommands(string image)
    {
        await using var container = new VaultBuilder(image)
            .WithRootToken(RootToken)
            .WithInitCommand("kv put secret/compatibility image=started")
            .Build();

        await container.StartAsync(TestContext.Current.CancellationToken);

        using var api = VaultApi.For(container);
        var value = await api.ReadSecretAsync("compatibility", "image", TestContext.Current.CancellationToken);

        Assert.Equal("started", value);
    }
}
