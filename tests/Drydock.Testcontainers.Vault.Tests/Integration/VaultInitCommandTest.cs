using Drydock.Testcontainers.Vault.Tests.Fixtures;
using Drydock.Testcontainers.Vault.Tests.Support;

namespace Drydock.Testcontainers.Vault.Tests.Integration;

public sealed class VaultInitCommandTest(InitializedVaultFixture fixture) : IClassFixture<InitializedVaultFixture>
{
    private readonly VaultContainer _container = fixture.Container;

    [Fact]
    public async Task EnablesSecretsEngine()
    {
        using var api = VaultApi.For(_container);

        var mounts = await api.GetAsync("v1/sys/mounts", TestContext.Current.CancellationToken);

        Assert.True(mounts.TryGetProperty("transit/", out _));
    }

    [Fact]
    public async Task CreatesKeyInsideTheEngineEnabledByTheEarlierCommand()
    {
        using var api = VaultApi.For(_container);
        var plaintext = Convert.ToBase64String("4111-demo"u8);

        var encrypted = await api.PostAsync($"v1/transit/encrypt/{InitializedVaultFixture.TransitKey}", new { plaintext }, TestContext.Current.CancellationToken);

        Assert.StartsWith("vault:v1:", encrypted.GetProperty("data").GetProperty("ciphertext").GetString());
    }

    [Fact]
    public async Task WritesToMountAddedAcrossSeparateCalls()
    {
        using var api = VaultApi.For(_container);

        var secret = await api.GetAsync("v1/legacy/db", TestContext.Current.CancellationToken);

        Assert.Equal("demo", secret.GetProperty("data").GetProperty("username").GetString());
    }

    [Fact]
    public async Task AcceptsCommandThatAlreadyNamesTheCli()
    {
        using var api = VaultApi.For(_container);

        var value = await api.ReadSecretAsync("app", "api-key", TestContext.Current.CancellationToken);

        Assert.Equal("from-init", value);
    }

    [Fact]
    public async Task WritesPolicyFromHereDocument()
    {
        using var api = VaultApi.For(_container);

        var policy = await api.GetAsync($"v1/sys/policies/acl/{InitializedVaultFixture.PolicyName}", TestContext.Current.CancellationToken);

        Assert.Contains(InitializedVaultFixture.PolicyPath, policy.GetProperty("data").GetProperty("policy").GetString());
    }
}
