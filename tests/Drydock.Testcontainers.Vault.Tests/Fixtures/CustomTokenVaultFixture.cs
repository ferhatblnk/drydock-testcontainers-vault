namespace Drydock.Testcontainers.Vault.Tests.Fixtures;

public sealed class CustomTokenVaultFixture : VaultFixture
{
    public const string RootToken = "integration-token";

    protected override VaultBuilder Configure(VaultBuilder builder)
    {
        return builder.WithRootToken(RootToken);
    }
}
