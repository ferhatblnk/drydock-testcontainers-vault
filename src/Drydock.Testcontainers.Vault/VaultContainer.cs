namespace Drydock.Testcontainers.Vault;

/// <summary>
/// A running HashiCorp Vault or OpenBao dev server.
/// </summary>
public sealed class VaultContainer : DockerContainer
{
    private const string MissingRootToken = "The configuration has no root token.";

    private readonly IReadOnlyList<string> _initCommands;

    private readonly string _rootToken;

    internal VaultContainer(VaultConfiguration configuration)
        : base(configuration)
    {
        _rootToken = configuration.RootToken ?? throw new ArgumentException(MissingRootToken, nameof(configuration));
        _initCommands = configuration.InitCommands;
    }

    /// <summary>
    /// Gets the address of the server as seen from the test host, for example <c>http://127.0.0.1:32768/</c>.
    /// </summary>
    /// <returns>The base address, including the randomly assigned host port.</returns>
    public string GetBaseAddress()
    {
        return new UriBuilder(Uri.UriSchemeHttp, Hostname, GetMappedPublicPort(VaultBuilder.VaultPort)).ToString();
    }

    /// <summary>
    /// Gets the root token that authenticates against the server.
    /// </summary>
    /// <returns>The root token set with <see cref="VaultBuilder.WithRootToken" />, or <see cref="VaultBuilder.DefaultRootToken" />.</returns>
    public string GetRootToken()
    {
        return _rootToken;
    }

    /// <summary>
    /// Starts the container, waits until the server is initialized, unsealed and active, then runs the init commands.
    /// </summary>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A task that completes when the server is ready and every init command has succeeded.</returns>
    /// <exception cref="VaultInitCommandException">An init command exited with a non-zero code.</exception>
    public override async Task StartAsync(CancellationToken ct = default)
    {
        await base.StartAsync(ct)
            .ConfigureAwait(false);

        if (_initCommands.Count == 0)
            return;

        var result = await ExecAsync(VaultInitScript.ToExecCommand(_initCommands), ct)
            .ConfigureAwait(false);

        if (result.ExitCode != 0)
            throw new VaultInitCommandException(result);
    }
}
