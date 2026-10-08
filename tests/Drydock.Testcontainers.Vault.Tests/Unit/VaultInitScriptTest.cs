using Drydock.Testcontainers.Vault.Helpers;

namespace Drydock.Testcontainers.Vault.Tests.Unit;

public sealed class VaultInitScriptTest
{
    [Fact]
    public void ComposeRunsCommandsInOrderAndStopsOnFirstError()
    {
        var script = VaultInitScript.Compose(["secrets enable transit", "kv put secret/app user=demo"]);

        Assert.Equal("set -e\nvault secrets enable transit\nvault kv put secret/app user=demo\n", script);
    }

    [Theory]
    [InlineData("status", "vault status")]
    [InlineData("  status  ", "vault status")]
    [InlineData("vault status", "vault status")]
    [InlineData("bao status", "bao status")]
    [InlineData("vaulted status", "vault vaulted status")]
    public void ComposeAddsTheCliNameOnlyWhenMissing(string command, string expected)
    {
        var script = VaultInitScript.Compose([command]);

        Assert.Equal($"set -e\n{expected}\n", script);
    }

    [Fact]
    public void ComposeKeepsHereDocumentsIntact()
    {
        var script = VaultInitScript.Compose(["policy write reader - <<EOF\npath \"secret/*\" { capabilities = [\"read\"] }\nEOF", "status"]);

        Assert.Equal("set -e\nvault policy write reader - <<EOF\npath \"secret/*\" { capabilities = [\"read\"] }\nEOF\nvault status\n", script);
    }

    [Fact]
    public void ToExecCommandWrapsTheScriptInOneShellCall()
    {
        var command = VaultInitScript.ToExecCommand(["status"]);

        Assert.Equal(["/bin/sh", "-c", "set -e\nvault status\n"], command);
    }

    [Fact]
    public void EnsureRejectsMissingCommands()
    {
        Assert.Throws<ArgumentNullException>(() => VaultInitScript.Ensure(null!, "commands"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void EnsureRejectsEmptyCommand(string? command)
    {
        var exception = Assert.Throws<ArgumentException>(() => VaultInitScript.Ensure(["status", command!], "commands"));

        Assert.Equal("commands", exception.ParamName);
    }
}
