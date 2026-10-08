using Drydock.Testcontainers.Vault.Tests.Support;

namespace Drydock.Testcontainers.Vault.Tests.Integration;

public sealed class VaultAppRoleTest
{
    private const string Role = "orders-api";

    [Fact]
    public async Task CredentialsReadThroughTheCliLogIn()
    {
        await using var container = new VaultBuilder(TestImages.Vault)
            .WithInitCommand("auth enable approle", $"write auth/approle/role/{Role} token_policies=default")
            .Build();

        await container.StartAsync(TestContext.Current.CancellationToken);

        var roleId = await container.ExecAsync(["vault", "read", "-field=role_id", $"auth/approle/role/{Role}/role-id"], TestContext.Current.CancellationToken);
        var secretId = await container.ExecAsync(["vault", "write", "-f", "-field=secret_id", $"auth/approle/role/{Role}/secret-id"], TestContext.Current.CancellationToken);

        using var api = VaultApi.For(container);
        var login = await api.PostAsync("v1/auth/approle/login", new { role_id = roleId.Stdout.Trim(), secret_id = secretId.Stdout.Trim() }, TestContext.Current.CancellationToken);

        Assert.False(string.IsNullOrEmpty(login.GetProperty("auth").GetProperty("client_token").GetString()));
    }
}
