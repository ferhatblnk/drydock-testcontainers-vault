using Drydock.Testcontainers.Vault.Tests.Support;

namespace Drydock.Testcontainers.Vault.Tests.Unit;

public sealed class VaultBuilderTest
{
    private readonly VaultBuilder _builder = new(TestImages.Vault);

    [Fact]
    public void WithRootTokenRejectsNull()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => _builder.WithRootToken(null!));

        Assert.Equal("rootToken", exception.ParamName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("my.token")]
    [InlineData("hvs.CAESIJ")]
    [InlineData("s.6w3CpXn5")]
    public void WithRootTokenRejectsWhatTheServerRejects(string rootToken)
    {
        var exception = Assert.Throws<ArgumentException>(() => _builder.WithRootToken(rootToken));

        Assert.Equal("rootToken", exception.ParamName);
    }

    [Fact]
    public void WithInitCommandRejectsEmptyCommand()
    {
        var exception = Assert.Throws<ArgumentException>(() => _builder.WithInitCommand("secrets enable transit", " "));

        Assert.Equal("commands", exception.ParamName);
    }

    [Fact]
    public void BuildUsesTheDefaultRootToken()
    {
        var container = _builder.Build();

        Assert.Equal(VaultBuilder.DefaultRootToken, container.GetRootToken());
    }

    [Fact]
    public void WithRootTokenLeavesTheOriginalBuilderUntouched()
    {
        var changed = _builder.WithRootToken("another-token");

        Assert.Equal("another-token", changed.Build().GetRootToken());
        Assert.Equal(VaultBuilder.DefaultRootToken, _builder.Build().GetRootToken());
    }

    [Fact]
    public void AcceptsAnImageInstance()
    {
        var container = new VaultBuilder(new DotNet.Testcontainers.Images.DockerImage(TestImages.OpenBao)).Build();

        Assert.Equal(TestImages.OpenBao, container.Image.FullName);
    }
}
