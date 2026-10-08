using Drydock.Testcontainers.Vault.Tests.Support;

namespace Drydock.Testcontainers.Vault.Tests.Fixtures;

public abstract class VaultFixture : IAsyncLifetime
{
    private readonly Lazy<VaultContainer> _container;

    protected VaultFixture()
    {
        _container = new Lazy<VaultContainer>(() => Configure(new VaultBuilder(TestImages.Vault)).Build());
    }

    public VaultContainer Container => _container.Value;

    public async ValueTask InitializeAsync()
    {
        await Container.StartAsync(TestContext.Current.CancellationToken);
    }

    public ValueTask DisposeAsync()
    {
        return Container.DisposeAsync();
    }

    protected virtual VaultBuilder Configure(VaultBuilder builder)
    {
        return builder;
    }
}
