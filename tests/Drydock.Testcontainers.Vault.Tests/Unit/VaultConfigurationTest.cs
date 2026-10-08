using DotNet.Testcontainers.Configurations;
using Drydock.Testcontainers.Vault.Helpers;

namespace Drydock.Testcontainers.Vault.Tests.Unit;

public sealed class VaultConfigurationTest
{
    [Fact]
    public void MergeKeepsTheOldTokenWhenTheNewValueHasNone()
    {
        var merged = new VaultConfiguration(new VaultConfiguration(rootToken: "first"), new VaultConfiguration());

        Assert.Equal("first", merged.RootToken);
    }

    [Fact]
    public void MergePrefersTheNewToken()
    {
        var merged = new VaultConfiguration(new VaultConfiguration(rootToken: "first"), new VaultConfiguration(rootToken: "second"));

        Assert.Equal("second", merged.RootToken);
    }

    [Fact]
    public void MergeAppendsInitCommandsInCallOrder()
    {
        var first = new VaultConfiguration(initCommands: ["one", "two"]);
        var unrelated = new VaultConfiguration(new VaultConfiguration());
        var second = new VaultConfiguration(initCommands: ["three"]);

        var merged = new VaultConfiguration(new VaultConfiguration(first, unrelated), second);

        Assert.Equal(["one", "two", "three"], merged.InitCommands);
    }

    [Fact]
    public void InitCommandsAreCopiedFromTheCaller()
    {
        string[] commands = ["one"];
        var configuration = new VaultConfiguration(initCommands: commands);

        commands[0] = "changed";

        Assert.Equal(["one"], configuration.InitCommands);
    }

    [Fact]
    public void SeedIsAnEmptyContainerConfiguration()
    {
        var seed = Assert.IsAssignableFrom<IContainerConfiguration>(ContainerConfigurationSeed.Value);

        Assert.Null(seed.Image);
        Assert.Null(seed.Environments);
        Assert.Null(seed.WaitStrategies);
    }
}
