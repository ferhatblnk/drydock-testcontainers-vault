namespace Drydock.Testcontainers.Vault;

internal sealed class VaultConnectionStringProvider : ContainerConnectionStringProvider<VaultContainer, VaultConfiguration>
{
    protected override string GetHostConnectionString()
    {
        return Container.GetBaseAddress();
    }
}
