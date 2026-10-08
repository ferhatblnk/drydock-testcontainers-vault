namespace Drydock.Testcontainers.Vault.Tests.Fixtures;

public sealed class InitializedVaultFixture : VaultFixture
{
    public const string TransitKey = "orders";

    public const string PolicyName = "reader";

    public const string PolicyPath = "secret/data/*";

    private const string Policy = $$"""
        policy write {{PolicyName}} - <<EOF
        path "{{PolicyPath}}" {
          capabilities = ["read"]
        }
        EOF
        """;

    protected override VaultBuilder Configure(VaultBuilder builder)
    {
        return builder
            .WithInitCommand("secrets enable transit", $"write -f transit/keys/{TransitKey}")
            .WithEnvironment("VAULT_LOG_LEVEL", "info")
            .WithInitCommand("secrets enable -path=legacy -version=1 kv", "kv put legacy/db username=demo")
            .WithInitCommand("vault kv put secret/app api-key=from-init", Policy);
    }
}
