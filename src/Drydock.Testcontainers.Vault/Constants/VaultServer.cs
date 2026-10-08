namespace Drydock.Testcontainers.Vault.Constants;

internal static class VaultServer
{
    public const string HealthPath = "/v1/sys/health";

    public static readonly string LoopbackAddress = "http://127.0.0.1:" + VaultBuilder.VaultPort.ToString(CultureInfo.InvariantCulture);
}
